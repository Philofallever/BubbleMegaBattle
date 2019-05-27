using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Config;
using DG.Tweening;
using GamePrefab;
using GameUI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

namespace Logic
{
    public struct Record
    {
        public int Level;
        public int Score;
    }

    /* StageNode负责舞台数据部分,StageBubble是预设实例,事实上二个数据合并到一起会
     * 更好,另外StageBubble最好是可以Recycle.
     */
    public class Manager : MonoBehaviour
    {
        [SerializeField, LabelText("舞台泡泡Parent")]
        private Transform _stageBubbParent;

        [SerializeField, LabelText("游戏配置")]
        private GameCfg _gameCfg;

        [Header("UI界面"), SerializeField, LabelText("启动界面")]
        private StartPanel _startPanel;

        [SerializeField, LabelText("游戏界面")]
        private GamePanel _gamePanel;

        private AudioSource _audioSource;

        // ReSharper disable once ConvertToAutoProperty
        public  GameCfg         GameCfg => _gameCfg;
        private Lazy<FlyBubble> _lazyFlyBubble;

        public static Manager               Instance        { get; private set; }
        public        int                   Level           { get; private set; }
        public        string                PlayerName      { get; private set; } // 玩家名字
        public        LinkedList<Record>    Records         { get; private set; } // 游戏记录
        public        int                   FlyCount        { get; private set; } // 发射次数
        public        StageAnchorData       StageAnchorData { get; private set; } // 舞台及锚点数据
        public        List<List<StageNode>> StageNodeData   { get; private set; } // 舞台Node数据

        private Dictionary<StageNode, HashSet<StageNode>> _parentRecords;  // 同色泡泡记录表(并查集)
        private List<StageBubble>                         _bubbsCache;     // Bubbles缓存
        private HashSet<StageNode>                        _nodesCache;     // Nodes缓存
        private HashSet<StageNode>                        _nodesPathCache; // Nodes缓存

        [RuntimeInitializeOnLoadMethod]
        public static void Initialize()
        {
            Application.targetFrameRate         = 60;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("zh-CN");
            Camera.main.orthographicSize        = (float) Screen.height / 2 / 100;
        }

        protected void Awake()
        {
            Instance       = this;
            _audioSource   = GetComponent<AudioSource>();
            _lazyFlyBubble = new Lazy<FlyBubble>(() => Instantiate(GameCfg.FlyBubble).GetComponent<FlyBubble>());
            StageNodeData  = new List<List<StageNode>>(GameConstant.StageRowCount);
            for (var i = 0; i < GameConstant.StageRowCount; ++i)
                StageNodeData.Add(new List<StageNode>(GameConstant.RowBubbMaxNum));
            _parentRecords  = new Dictionary<StageNode, HashSet<StageNode>>();
            _bubbsCache     = new List<StageBubble>();
            _nodesCache     = new HashSet<StageNode>();
            _nodesPathCache = new HashSet<StageNode>();
            LoadData();
        }

        #region 对外接口

        public void StartGame(int lvl)
        {
            _gamePanel.gameObject.SetActive(true);
            InitLevelData(lvl);
        }

        [Button]
        public void InitLevelData(int lvl)
        {
            Level = lvl;
            var tunning = GameCfg.LevelTunnings[lvl];
            StageAnchorData = new StageAnchorData(tunning.StageType);
            InitLevelStage();
        }

        public void ToggleBgm()
        {
            if (_audioSource.isPlaying)
                _audioSource.Stop();
            else
                _audioSource.Play();
        }

        #endregion

        #region Logic

        private void InitLevelStage()
        {
            // 数据
            foreach (var row in StageNodeData)
                row.Clear();
            var lvlTunning = GameCfg.LevelTunnings[Level];
            var initBubbs  = lvlTunning.InitBubles;
            for (var row = 0; row < StageNodeData.Count; ++row)
            {
                var rowCount = StageAnchorData.GetRowAnchorsCount(row);
                for (var col = 0; col < rowCount; ++col)
                {
                    var cfgClr = row < initBubbs.Length && col < initBubbs[row].Length ? initBubbs[row][col] : BubbType.Empty;
                    var node   = new StageNode {Row = row, Col = col, BubbType = cfgClr, AnchorPos = StageAnchorData[row, col]};
                    StageNodeData[row].Add(node);
                }
            }

            foreach (var rowNodes in StageNodeData)
            {
                foreach (var node in rowNodes)
                    SpawnStageBubble(node);
            }

            _gamePanel.SpawnWaitBubble();
        }

        public void SpawnFlyBubble(BubbType type, Vector2 flyDir, Vector2 position)
        {
            ++FlyCount;
            _lazyFlyBubble.Value.Respawn(type, flyDir, position);
        }

        // 在某个Node生成泡泡,并更新并查集
        private void SpawnStageBubble(StageNode node)
        {
            if (node.BubbType == BubbType.Empty || node.BubbType == BubbType.Colorful) return;

            // 遍历周围的泡泡,重设parent
            foreach (var sideNode in node)
            {
                // 同色的合并
                if (sideNode?.BubbType == node.BubbType)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    if (sideNode.ParentNode == null && node.ParentNode == null)
                    {
                        node.ParentNode      = node;
                        sideNode.ParentNode  = node;
                        _parentRecords[node] = new HashSet<StageNode>() {node, sideNode};
                    }
                    else if (sideNode.ParentNode != null && node.ParentNode == null)
                    {
                        node.ParentNode = sideNode.ParentNode;
                        _parentRecords[sideNode.ParentNode].Add(node);
                    }
                    else if (sideNode.ParentNode == null && node.ParentNode != null)
                    {
                        sideNode.ParentNode = node.ParentNode;
                        _parentRecords[node.ParentNode].Add(sideNode);
                    }
                    else if (sideNode.ParentNode != node.ParentNode)
                        CombineParentSet(sideNode.ParentNode, node.ParentNode);
                }
            }

            // 如果周围没同色的
            if (node.ParentNode == null)
            {
                node.ParentNode      = node;
                _parentRecords[node] = new HashSet<StageNode>() {node};
            }

            var stageBubble = Instantiate(GameCfg.StageBubble, node.AnchorPos, Quaternion.identity, _stageBubbParent).GetComponent<StageBubble>();
            stageBubble.Respawn(node);
        }

        // 合并parent
        private void CombineParentSet(StageNode parent1, StageNode parent2)
        {
            var parent = parent2.Row < parent1.Row ? parent2 : parent1;
            var child  = parent2.Row < parent1.Row ? parent1 : parent2;
            foreach (var node in _parentRecords[child])
                node.ParentNode = parent;
            _parentRecords[parent].UnionWith(_parentRecords[child]);
            _parentRecords.Remove(child);
        }

        #region 泡泡碰撞,消除,下移:碰撞后,判断能消除,消除后判断是否能下移,结束后产生新的待发射泡泡

        // 碰撞到泡泡回调
        public void OnCollideStageBubble(Collision2D collision)
        {
            var involveBubb  = collision.gameObject.GetComponent<StageBubble>();
            var involveNode  = involveBubb.StageNode;
            var contactPoint = collision.GetContact(0).point;

            StageNode targetNode = null;
            foreach (var sideNode in involveNode)
            {
                if (sideNode == null || sideNode.BubbType != BubbType.Empty)
                    continue;

                if (targetNode == null)
                    targetNode = sideNode;
                else
                {
                    var distancePre = Vector2.SqrMagnitude(targetNode.AnchorPos - contactPoint);
                    var distanceNow = Vector2.SqrMagnitude(sideNode.AnchorPos - contactPoint);

                    if (distanceNow < distancePre)
                        targetNode = sideNode;
                }
            }

            if (targetNode == null)
            {
                SetLevelResult(LevelResult.FailToFindNode);
                return;
            }

            targetNode.BubbType = _lazyFlyBubble.Value.BubbType == BubbType.Colorful ? involveNode.BubbType : _lazyFlyBubble.Value.BubbType;
            OnFindNodeSuccAfterCollide(targetNode);
        }

        // 碰到Stage上边缘
        public void OnCollideStageTopEdge(Collision2D collision)
        {
            var       contactPoint = collision.contacts[0].point;
            StageNode targetNode   = null;
            foreach (var rowNode in StageNodeData[0])
            {
                if (rowNode.BubbType != BubbType.Empty) continue;

                if (targetNode == null)
                    targetNode = rowNode;
                else
                {
                    var disPre = Vector2.SqrMagnitude(targetNode.AnchorPos - contactPoint);
                    var disNow = Vector2.SqrMagnitude(rowNode.AnchorPos - contactPoint);
                    if (disNow < disPre)
                        targetNode = rowNode;
                }
            }

            if (targetNode == null)
            {
                SetLevelResult(LevelResult.FailToFindNode);
                return;
            }

            if (_lazyFlyBubble.Value.BubbType == BubbType.Colorful)
            {
                var weights = GameCfg.LevelTunnings[Level].StageBubbWeights;
                targetNode.BubbType = weights.SelectByWeight();
            }
            else
                targetNode.BubbType = _lazyFlyBubble.Value.BubbType;

            OnFindNodeSuccAfterCollide(targetNode);
        }

        private void OnFindNodeSuccAfterCollide(StageNode stageNode)
        {
            SpawnStageBubble(stageNode);
            var wipeCount = WipeBubbleAfterCollide(stageNode);
            if (wipeCount > 0)
            {
                // jiafen 

                // tongguan?
            }

            // 下移

            var cfgTimes = GameCfg.LevelTunnings[Level].MoveDownFlyTimes;
            if (cfgTimes != 0 && FlyCount % cfgTimes == 0)
            {
                if (CanMoveDown())
                {
                    var leftBubbs = _stageBubbParent.childCount - wipeCount;
                    StartCoroutine(MoveDownWait(leftBubbs));
                }
                else
                    SetLevelResult(LevelResult.FailToMoveDown);
            }
            else
                _gamePanel.SpawnWaitBubble();
        }

        // 消除泡泡,返回消除的个数
        private int WipeBubbleAfterCollide(StageNode stageNode)
        {
            var wipeNodes = _parentRecords[stageNode.ParentNode];
            if (wipeNodes.Count < GameConstant.BubbWipeThreshold)
                return 0;

            // 没有挂载点的泡泡
            for (var row = 0; row < GameConstant.StageRowCount; ++row)
            {
                for (var col = 0; col < StageNodeData[row].Count; ++col)
                {
                    var node = StageNodeData[row][col];
                    GenLinkDataByBubb(node);
                }
            }

            _nodesCache.Clear(); // GenLinkDataByBubb 用到了缓存
            foreach (var wipe in wipeNodes)
            {
                if (wipe.ParentNode == wipe)
                    _parentRecords.Remove(wipe);

                wipe.BubbType   = BubbType.Empty;
                wipe.ParentNode = null;
            }

            _stageBubbParent.GetComponentsInChildren<StageBubble>(_bubbsCache);
            _bubbsCache.RemoveAll(bubb => !wipeNodes.Contains(bubb.StageNode));
            foreach (var bubb in _bubbsCache)
                bubb.PlayWipeAnim();

            return wipeNodes.Count;

            #region 局部函数

            // 从某个泡泡生成连接数据
            void GenLinkDataByBubb(StageNode node)
            {
                if (node.BubbType == BubbType.Empty || _nodesCache.Contains(node) || wipeNodes.Contains(node))
                    return;

                var isLink = IsBubbLinkToTop(node, _nodesPathCache);
                if (isLink)
                    _nodesCache.UnionWith(_nodesPathCache);
                else
                    wipeNodes.UnionWith(_nodesPathCache);
                _nodesPathCache.Clear();
            }

            // 判断某个泡泡是否连到顶部
            bool IsBubbLinkToTop(StageNode node, HashSet<StageNode> path)
            {
                if (!path.Add(node) || wipeNodes.Contains(node))
                    return false;

                if (node.Row == 0 || wipeNodes.Contains(node))
                    return true;

                foreach (var sideNode in node)
                {
                    if (sideNode == null || sideNode.BubbType == BubbType.Empty) continue;

                    var isLink = IsBubbLinkToTop(sideNode, path);
                    if (isLink)
                        return true;
                }

                return false;
            }

            #endregion
        }

        private bool CanMoveDown()
        {
            var lastMoveRow = GameConstant.StageRowCount - GameConstant.MoveDownRowNum;
            if (StageNodeData[lastMoveRow].Exists(node => node.BubbType != BubbType.Empty))
                return false;
            else
                return true;
        }

        private IEnumerator MoveDownWait(int leftBubbCount)
        {
            yield return new WaitWhile(() => _stageBubbParent.childCount > leftBubbCount);

            MoveBubbDown();
            _gamePanel.SpawnWaitBubble();
        }

        private void MoveBubbDown()
        {
            var count       = GameConstant.MoveDownRowNum;
            var lastMoveRow = GameConstant.StageRowCount - count;
            var emptyRows   = StageNodeData.GetRange(lastMoveRow, count);
            StageNodeData.RemoveRange(lastMoveRow, count);
            StageNodeData.InsertRange(0, emptyRows);

            // 更新锚点数据
            for (var row = 0; row < StageNodeData.Count; ++row)
            {
                var rowCount = StageAnchorData.GetRowAnchorsCount(row);
                for (var col = 0; col < rowCount; ++col)
                {
                    var node = StageNodeData[row][col];
                    node.Row       = row;
                    node.Col       = col;
                    node.AnchorPos = StageAnchorData[row, col];
                }
            }

            // 重设现有泡泡位置
            _stageBubbParent.GetComponentsInChildren<StageBubble>(_bubbsCache);
            foreach (var bubble in _bubbsCache)
                bubble.transform.position = bubble.StageNode.AnchorPos;

            // 为空生成新的泡泡
            for (var row = count - 1; row >= 0; --row)
            {
                for (var col = 0; col < StageNodeData[row].Count; ++col)
                {
                    var node = StageNodeData[row][col];

                    var bubbType = GameCfg.LevelTunnings[Level].StageBubbWeights.SelectByWeight();
                    // 必需填充或者随机到填充,则填充
                    if (IsMustFillNode(node) || GameCfg.LevelTunnings[Level].UnNecessaryFillRatio >= UnityRandom.value)
                    {
                        node.BubbType = bubbType;
                        SpawnStageBubble(node);
                    }
                }
            }
        }

        private bool IsMustFillNode(StageNode node)
        {
            var downLeft  = node.GetDownLeft();
            var downRight = node.GetDownRight();

            if (downLeft != null && downLeft.BubbType != BubbType.Empty)
            {
                var downLeftUpLeft = downLeft.GetUpLeft();
                if (downLeftUpLeft == null || downLeftUpLeft.BubbType == BubbType.Empty)
                    return true;
            }

            if (downRight != null && downRight.BubbType != BubbType.Empty)
            {
                var downRightUpRight = downRight.GetUpRight();
                if (downRightUpRight == null || downRightUpRight.BubbType == BubbType.Empty)
                    return true;
            }

            return false;
        }

        #endregion

        private void SetLevelResult(LevelResult result)
        {
            print($"game over because of {result}");
        }

        private void LoadData()
        {
            Records = new LinkedList<Record>();
            var fileName = Path.Combine(Application.persistentDataPath, "save.data");
            if (!File.Exists(Application.persistentDataPath)) return;

            using (var fileStream = File.OpenRead(fileName))
            {
                using (var reader = new BinaryReader(fileStream))
                {
                    PlayerName = reader.ReadString();
                    var count = reader.ReadInt32();
                    for (var i = 0; i < count; ++i)
                    {
                        var record = new Record() {Level = reader.ReadInt32(), Score = reader.ReadInt32()};
                        Records.AddFirst(record);
                    }
                }
            }
        }

        private void SaveData()
        {
            var fileName = Path.Combine(Application.persistentDataPath, "save.data");
            using (var fileStream = File.OpenWrite(fileName))
            {
                using (var writer = new BinaryWriter(fileStream))
                {
                    writer.Write(PlayerName);
                    writer.Write(Records.Count);
                    foreach (var record in Records)
                    {
                        writer.Write(record.Level);
                        writer.Write(record.Score);
                    }
                }
            }
        }

        #endregion
    }
}