using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Config;
using DG.Tweening;
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

        public        GameCfg               GameCfg;
        public        GameObject            Ball;
        public static Manager               Instance        { get; private set; }
        public        int                   Level           { get; private set; }
        public        StageAnchorData       StageAnchorData { get; private set; }
        public        List<List<StageNode>> StageNodeData   { get; private set; }

        [RuntimeInitializeOnLoadMethod]
        public static void Initialize()
        {
            Application.targetFrameRate         = 60;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("zh-CN");
        }

        protected void Awake()
        {
            Instance      = this;
            StageNodeData = new List<List<StageNode>>(Constant.StageRowCount);
            for (var i = 0; i < Constant.StageRowCount; ++i)
                StageNodeData.Add(new List<StageNode>(Constant.RowBubbMaxNum));
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
                    StageNodeData[row].Add(new StageNode {Row = row, Col = col, BubbType = cfgClr, AnchorPos = StageAnchorData[row, col]});
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

            var src         = GameCfg.StageBubble;
            var stageBubble = Instantiate(src, node.AnchorPos, Quaternion.identity, _stageBubbParent).GetComponent<StageBubble>();
            stageBubble.SetBubbleType(node.BubbType);
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
        }
    }
}