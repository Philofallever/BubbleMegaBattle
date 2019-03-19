using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Config;
using GamePrefab;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Logic
{
    public class Manager : MonoBehaviour
    {
        [SerializeField, LabelText("舞台泡泡Parent")]
        private Transform _stageBubbParent;

        [SerializeField, LabelText("随机泡泡")]
        private RandomBubble _randomBubble;

        [SerializeField, LabelText("待发射的泡泡")]
        private WaitingBubble _waitingBubble;

        [SerializeField, LabelText("舞台泡泡")]
        private GameObject _stageBubble;

        [SerializeField, LabelText("飞行中的球")]
        private GameObject _flyBubble;

        [SerializeField, LabelText("游戏配置")]
        private GameCfg _gameCfg;

        // ReSharper disable once ConvertToAutoProperty
        public  GameCfg         GameCfg => _gameCfg;
        private Lazy<FlyBubble> _lazyFlyBubble;

        public static Manager               Instance        { get; private set; }
        public        int                   Level           { get; private set; }
        public        StageAnchorData       StageAnchorData { get; private set; }
        public        List<List<StageNode>> StageNodeData   { get; private set; }

        private Dictionary<StageNode, HashSet<StageNode>> _parentRecords; // 同色泡泡记录表(并查集)
        private List<StageBubble>                         _bubbsCache;    // Bubbles缓存
        private HashSet<StageNode>                        _nodesCache;    // Nodes缓存
        private HashSet<StageNode>                        _nodesCache2;   // Nodes缓存

        [RuntimeInitializeOnLoadMethod]
        public static void Initialize()
        {
            Application.targetFrameRate         = 60;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("zh-CN");
        }

        protected void Awake()
        {
            _lazyFlyBubble = new Lazy<FlyBubble>(() => Instantiate(_flyBubble).GetComponent<FlyBubble>());
            Instance       = this;
            StageNodeData  = new List<List<StageNode>>(GameConstant.StageRowCount);
            for (var i = 0; i < GameConstant.StageRowCount; ++i)
                StageNodeData.Add(new List<StageNode>(GameConstant.RowBubbMaxNum));
            _parentRecords = new Dictionary<StageNode, HashSet<StageNode>>();
            _bubbsCache    = new List<StageBubble>();
            _nodesCache    = new HashSet<StageNode>();
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
                    if (cfgClr == BubbType.Empty) continue;

                    node.ParentNode      = node;
                    _parentRecords[node] = new HashSet<StageNode> {node};
                }
            }

            foreach (var rowNodes in StageNodeData)
            {
                foreach (var node in rowNodes)
                    SpawnStageBubble(node);
            }

            SpawnRandomBubble();
            SpawnWaitBubble();
        }

        private void SpawnStageBubble(StageNode node)
        {
            if (node.BubbType == BubbType.Empty || node.BubbType == BubbType.Colorful) return;

            var stageBubble = Instantiate(_stageBubble, node.AnchorPos, Quaternion.identity, _stageBubbParent).GetComponent<StageBubble>();
            stageBubble.Respawn(node);

            // 遍历周围的泡泡,重设parent
            foreach (var sideNode in node)
            {
                if (sideNode == null || sideNode.BubbType != node.BubbType)
                    continue;

                if (sideNode.ParentNode == node.ParentNode)
                    continue;

                CombineParentSet(sideNode.ParentNode, node.ParentNode);
            }

            //foreach (var record in _parentRecords)
            //{
            //    print($"{record.Key.BubbType}:({record.Key.Row},{record.Key.Col}) => {record.Value.Count}");
            //}
        }

        [Button]
        private void SpawnRandomBubble()
        {
            _randomBubble.Respawn();
        }

        [Button]
        private void SpawnWaitBubble()
        {
            StartCoroutine(WaitAndSpawnRandomBubb());

            IEnumerator WaitAndSpawnRandomBubb()
            {
                _randomBubble.PlayMoveAnim(out var animDuration);
                yield return new WaitForSeconds(animDuration);

                _waitingBubble.Respawn(_randomBubble.BubbType, out animDuration);

                yield return new WaitForSeconds(animDuration);

                SpawnRandomBubble();
            }
        }

        public void SpawnFlyBubble()
        {
            var type   = _waitingBubble.BubbType;
            var flyDir = _waitingBubble.FlyDirection;
            _lazyFlyBubble.Value.Respawn(type, flyDir, _waitingBubble.transform.position);
        }

        // 碰撞后回调
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
                Debug.LogError("找不到附件的空泡泡位置!");
                return;
            }

            targetNode.BubbType   = _lazyFlyBubble.Value.BubbType == BubbType.Colorful ? involveNode.BubbType : _lazyFlyBubble.Value.BubbType;
            targetNode.ParentNode = targetNode;
            _parentRecords.Add(targetNode, new HashSet<StageNode> {targetNode});
            SpawnStageBubble(targetNode);
            WipeBubbleAfterCollide(targetNode.ParentNode);
        }


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
                Debug.LogError("找不到附件的空泡泡位置!");
                return;
            }

            targetNode.BubbType = _lazyFlyBubble.Value.BubbType == BubbType.Colorful
                                      ? (BubbType) UnityEngine.Random.Range((int) BubbType.Orange, (int) BubbType.Colorful)
                                      : _lazyFlyBubble.Value.BubbType;
            targetNode.ParentNode = targetNode;
            _parentRecords.Add(targetNode, new HashSet<StageNode> {targetNode});
            SpawnStageBubble(targetNode);
            WipeBubbleAfterCollide(targetNode.ParentNode);
        }

        private void WipeBubbleAfterCollide(StageNode parentNode)
        {
            if (_parentRecords[parentNode].Count < GameConstant.BubbWipeThreshold)
            {
                SpawnWaitBubble();
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
                }
            }

            _nodesCache.Clear();
            _nodesCache2.Clear();
            // 消除的泡泡
            _stageBubbParent.GetComponentsInChildren(_bubbsCache);
            _bubbsCache.RemoveAll(bubb => !wipeNodes.Contains(bubb.StageNode));

            foreach (var bubble in _bubbsCache)
                bubble.PlayWipeAnim();

            _bubbsCache.Clear();
            SpawnWaitBubble();

            // 用到了cache所以写成局部函数
            bool IsBubbLinkToTop(StageNode node)
            {
                if (node == null || node.BubbType == BubbType.Empty) return false;

                _nodesCache2.Add(node);
                if (node.Row == 0)
                {
                    _nodesCache.Add(node);
                    return true;
                }

                if (_nodesCache.Contains(node))
                    return true;

                // BUG 两个泡泡会一直递归
                foreach (var sideNode in node)
                {
                    if (sideNode == null || sideNode.BubbType == BubbType.Empty || _nodesCache2.Contains(node)) continue;

                    var isLink = IsBubbLinkToTop(sideNode);
                    if (isLink)
                    {
                        _nodesCache.Add(node);
                        return true;
                    }
                }

                return false;
            }


            //_bubbsCache.Add(Array.Find(stageBubbs, bubb => bubb.Row == node.Row && bubb.Col == node.Row));
        }


        private void RefreshStageNode(int row, int col, BubbType type)
        {
            var stageNode = StageNodeData[row][col];
            if (stageNode.BubbType == BubbType.Empty)
            {
                stageNode.BubbType = type;
                SpawnStageBubble(stageNode);
            }

            stageNode.BubbType = type;
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
    }
}