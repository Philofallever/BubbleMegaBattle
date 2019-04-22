using System.Collections.Generic;
using Logic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Config
{
    [TypeInfoBox("关卡难度设定")]
    public class LevelTunning
    {
        [LabelText("舞台样式")]
        public StageType StageType;

        [LabelText("初始填充泡泡")]
        public BubbType[][] InitBubles;

        [LabelText("下移需要发射次数*"), Tooltip("0表示不会下移")]
        public int MoveDownFlyTimes;

        [LabelText("待发射泡泡生成权重表")]
        public Dictionary<BubbType, int> WaitBubbWeights;

        private Dictionary<BubbType, int> _stageBubbWeights;

        public Dictionary<BubbType, int> StageBubbWeights

        {
            get
            {
                if (_stageBubbWeights == null)
                {
                    _stageBubbWeights = new Dictionary<BubbType, int>(WaitBubbWeights);
                    _stageBubbWeights.Remove(BubbType.Colorful);
                }

                return _stageBubbWeights;
            }
        }

        [LabelText("下移时非必需位填充几率"),]
        public float UnNecessaryFillRatio;
    }

    public class GameCfg : SerializedScriptableObject
    {
        [TabGroup("通用配置"), LabelText("游戏规则"), Multiline(5)]
        public string RuleDesc;

        [TabGroup("通用配置"), LabelText("飞行气泡速度")]
        public float FlyBubbleSpeed;

        [TabGroup("通用配置"), LabelText("额外奖励分")]
        public int ExtraGrade;

        [TabGroup("关卡配置"), LabelText("关卡最大数*")]
        public int MaxLevel;

        [TabGroup("关卡配置"), LabelText("关卡配置")]
        public LevelTunning[] LevelTunnings;

        [TabGroup("游戏资源"), LabelText("泡泡精灵")]
        public Sprite[] BubbSprites;

        [TabGroup("游戏资源"), LabelText("舞台泡泡")]
        public GameObject StageBubble;

        [TabGroup("游戏资源"), LabelText("飞行中的球")]
        public GameObject FlyBubble;

        [Button("保存", ButtonSizes.Medium)]
        private void Save()
        {
            AssetDatabase.SaveAssets();
        }
    }
}