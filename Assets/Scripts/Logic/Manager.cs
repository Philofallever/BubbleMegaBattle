using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
        private GameObject _startPanel;

        [SerializeField, LabelText("游戏界面")]
        private GamePanel _gamePanel;

        // ReSharper disable once ConvertToAutoProperty
        public  GameCfg         GameCfg => _gameCfg;
        private Lazy<FlyBubble> _lazyFlyBubble;

        public static Manager               Instance        { get; private set; }
        public        int                   Level           { get; private set; }
        public        int                   FlyCount        { get; private set; } // 发射次数
        public        StageAnchorData       StageAnchorData { get; private set; } // 舞台及锚点数据
        public        List<List<StageNode>> StageNodeData   { get; private set; } // 舞台Node数据

        [SerializeField]
        private Dictionary<StageNode, HashSet<StageNode>> _parentRecords;  // 同色泡泡记录表(并查集)
        private List<StageBubble>                         _bubbsCache;     // Bubbles缓存
        private HashSet<StageNode>                        _nodesCache;     // Nodes缓存
        private HashSet<StageNode>                        _nodesPathCache; // Nodes缓存

        [RuntimeInitializeOnLoadMethod]
        public static void Initialize()
        {
            Application.targetFrameRate         = 60;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("zh-CN");
        }

        protected void Awake()
        {
            Instance       = this;
            _lazyFlyBubble = new Lazy<FlyBubble>(() => Instantiate(GameCfg.FlyBubble).GetComponent<FlyBubble>());
            StageNodeData  = new List<List<StageNode>>(GameConstant.StageRowCount);
            for (var i = 0; i < GameConstant.StageRowCount; ++i)
                StageNodeData.Add(new List<StageNode>(GameConstant.RowBubbMaxNum));
            _parentRecords  = new Dictionary<StageNode, HashSet<StageNode>>();
            _bubbsCache     = new List<StageBubble>();
            _nodesCache     = new HashSet<StageNode>();
            _nodesPathCache = new HashSet<StageNode>();
        }

        [Button]
        public void StartLevel(int lvl)
        {
            Level = lvl;
            var tunning = GameCfg.LevelTunnings[lvl];
            StageAnchorData = new StageAnchorData(tunning.StageType);
            InitLevelStage();
        }

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
                    SpawnStageBubble(node,true);
                }
            }

            //foreach (var rowNodes in StageNodeData)
            //{
            //    foreach (var node in rowNodes)
            //        SpawnStageBubble(node);
            //}

            _gamePanel.SpawnWaitBubble();
        }

        public void SpawnFlyBubble(BubbType type, Vector2 flyDir, Vector2 position)
        {
            ++FlyCount;
            _lazyFlyBubble.Value.Respawn(type, flyDir, position);
        }

        // 在某个Node生成泡泡,并更新并查集
        private void SpawnStageBubble(StageNode node,bool ignoreLow = false)
        {
            if (node.BubbType == BubbType.Empty || node.BubbType == BubbType.Colorful) return;

            // 遍历周围的泡泡,重设parent
            foreach (var sideNode in node)
            {


                if (sideNode?.BubbType == node.BubbType)
                {
                    // 已经是一个集合
                    if (sideNode.ParentNode == node.ParentNode)
                        continue;

                    if (node.ParentNode == null)
                    {
                        node.ParentNode = sideNode.ParentNode;
                        _parentRecords[node.ParentNode].Add(node);
                    }
                    else
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
            if (parent1== null || parent2 == null)
            {
                print($"{parent1?.Row} {parent1?.Col}");
                print($"{parent2?.Row} {parent2?.Col}");

            }


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
            SpawnStageBubble(targetNode);
            WipeBubbleAfterCollide(targetNode.ParentNode);
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

            targetNode.BubbType = _lazyFlyBubble.Value.BubbType == BubbType.Colorful
                                      ? BubbTypeUtil.GetRandomStageType()
                                      : _lazyFlyBubble.Value.BubbType;
            SpawnStageBubble(targetNode);
            WipeBubbleAfterCollide(targetNode.ParentNode);
        }

        private void WipeBubbleAfterCollide(StageNode parentNode)
        {
            if (_parentRecords[parentNode].Count < GameConstant.BubbWipeThreshold)
            {
                _gamePanel.SpawnWaitBubble();
                return;
            }

            //  要消除的泡泡
            var wipeNodes = _parentRecords[parentNode];
            _parentRecords.Remove(parentNode);
            foreach (var node in wipeNodes)
            {
                node.BubbType   = BubbType.Empty;
                node.ParentNode = null;
            }

            // 没有挂载点的泡泡
            for (var row = 0; row < GameConstant.StageRowCount; ++row)
            {
                for (var col = 0; col < StageNodeData[row].Count; ++col)
                {
                    var node = StageNodeData[row][col];
                    if (node.BubbType == BubbType.Empty) continue;

                    if (!IsBubbLinkToTop(node))
                    {
                        var record = _parentRecords[node.ParentNode];
                        record.Remove(node);
                        // 恰好是并查集父节点
                        if (node.ParentNode == node)
                        {
                            _parentRecords.Remove(node);

                            if (record.Count != 0)
                            {
                                StageNode newParent = null;
                                foreach (var childNode in record)
                                {
                                    if (newParent == null) newParent = childNode;

                                    childNode.ParentNode = newParent;
                                }

                                _parentRecords.Add(newParent, record);
                            }
                        }

                        node.BubbType   = BubbType.Empty;
                        node.ParentNode = null;
                        wipeNodes.Add(node);
                    }

                    _nodesPathCache.Clear();
                }
            }

            _nodesCache.Clear();
            // 消除的泡泡
            _stageBubbParent.GetComponentsInChildren(_bubbsCache);
            _bubbsCache.RemoveAll(bubb => !wipeNodes.Contains(bubb.StageNode));

            foreach (var bubble in _bubbsCache)
                bubble.PlayWipeAnim();

            _bubbsCache.Clear();

            // 用到了cache所以写成局部函数
            bool IsBubbLinkToTop(StageNode node)
            {
                if (node == null || node.BubbType == BubbType.Empty) return false;

                if (!_nodesPathCache.Add(node)) // 已经查找过的点
                    return false;

                if (node.Row == 0) // 第一排
                {
                    _nodesCache.Add(node);
                    return true;
                }

                if (_nodesCache.Contains(node))
                    return true;

                foreach (var sideNode in node)
                {
                    if (sideNode == null || sideNode.BubbType == BubbType.Empty) continue;

                    var isLink = IsBubbLinkToTop(sideNode);
                    if (isLink)
                    {
                        _nodesCache.Add(node);
                        return true;
                    }
                }

                return false;
            }
        }

        //private void TryMoveDown()
        //{
        //    if (FlyCount % GameCfg.LevelTunnings[Level].MoveDownFlyTimes != 0) return;

        //    // 如果不能下移则游戏结束
        //    var lastRow = StageNodeData[GameConstant.StageRowCount - GameConstant.MoveDowRowNum - 1];
        //    if (lastRow.Exists(node => node.BubbType != BubbType.Empty))
        //    {
        //        SetLevelResult(LevelResult.FailToMoveDown);
        //        return;
        //    }

        //    _stageBubbParent.GetComponentsInChildren<StageBubble>(_bubbsCache);
        //    for (var row = GameConstant.StageRowCount - 1; row >= GameConstant.MoveDowRowNum; --row)
        //    {
        //        for (var col = 0; col < StageNodeData[row].Count; ++col)
        //        {
        //            StageNodeData[row][col].ParentNode = StageNodeData[row - 2][col].ParentNode;
        //            StageNodeData[row][col].BubbType   = StageNodeData[row - 2][col].BubbType;
        //        }
        //    }
        //    // 生成新泡泡

        //    for (var row = GameConstant.RowBubbMinNum - 1; row >= 0; --row)
        //    {
        //        var blowRow = StageNodeData[row + 1];
        //        foreach (var blowNode in blowRow)
        //        {
        //            var upLeft  = blowNode.GetUpLeft();
        //            var upRight = blowNode.GetUpRight();

        //            if (blowNode.BubbType == BubbType.Empty)
        //            {
        //                if (upLeft?.BubbType == BubbType.Empty)
        //                {
        //                    upLeft.BubbType = UnityRandom.value > 1f / (Level + 1) ? GetRandomStageBubbType() : BubbType.Empty;

        //                    if (upRight == null && upLeft.BubbType == BubbType.Empty)
        //                        upLeft.BubbType = GetRandomStageBubbType();

        //                    if (upLeft.BubbType != BubbType.Empty)
        //                    {
        //                        upLeft.ParentNode      = upRight;
        //                        _parentRecords[upLeft] = new HashSet<StageNode>() {upLeft};
        //                    }
        //                }

        //                if (upRight?.BubbType == BubbType.Empty)
        //                {
        //                    upRight.BubbType = UnityRandom.value < 1f / (Level + 1) ? upLeft?.BubbType ?? BubbType.Empty : BubbType.Empty;
        //                    if (upLeft == null && upRight.BubbType == BubbType.Empty)
        //                        upRight.BubbType = GetRandomStageBubbType();

        //                    if (upRight.BubbType != BubbType.Empty)
        //                    {
        //                        upRight.ParentNode      = upRight;
        //                        _parentRecords[upRight] = new HashSet<StageNode>() {upRight};
        //                    }
        //                }
        //            }
        //            else
        //            {

        //                if (upLeft?.BubbType == BubbType.Empty)
        //                {
        //                    upLeft.BubbType = GetRandomStageBubbType();
        //                    upLeft.ParentNode      = upRight;
        //                    _parentRecords[upLeft] = new HashSet<StageNode>() {upLeft};
        //                }

        //                if(upRight?.BubbType == BubbType.Empty)


        //            }
        //        }
        //    }
        //    foreach (var bubble in _bubbsCache)
        //        bubble.MoveDown();
        //}

        #endregion

        private void SetLevelResult(LevelResult result)
        {
        }
    }
}