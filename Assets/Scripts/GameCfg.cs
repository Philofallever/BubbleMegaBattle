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

        [LabelText("关卡名")]
        public string Name;

        [LabelText("初始填充泡泡"), TableMatrix(HorizontalTitle = "列数:固定10", VerticalTitle = "行:按需填", Transpose = true)]
        public BubbType[,] InitBubles;

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

        [LabelText("下移时非必需位填充几率")]
        public float UnNecessaryFillRatio;
    }

    public class GameCfg : SerializedScriptableObject
    {
        [TabGroup("通用配置"), LabelText("游戏规则"), Multiline(5)]
        public string RuleDesc;

        [TabGroup("通用配置"), LabelText("飞行气泡速度")]
        public float FlyBubbleSpeed;

        [TabGroup("通用配置"), LabelText("额外消除数")]
        public int[] ExtraWipes;

        [TabGroup("通用配置"), LabelText("额外消除奖励分")]
        public int[] ExtraScores;

        [TabGroup("通用配置"), LabelText("关卡名称格式")]
        public string LevelNameFmt;

        [TabGroup("通用配置"), LabelText("记录标题"), TextArea]
        public string RecordTitle;

        [TabGroup("通用配置"), LabelText("记录条目"), TextArea]
        public string RecordEntry;

        [TabGroup("通用配置"), LabelText("字符串")]
        public TextAsset Light;

        [TabGroup("关卡配置"), LabelText("关卡配置"), ListDrawerSettings(ShowIndexLabels = true)]
        public LevelTunning[] LevelTunnings;

        [TabGroup("游戏资源"), LabelText("背景")]
        public Sprite[] Backgrounds;

        [TabGroup("游戏资源"), LabelText("泡泡精灵")]
        public Sprite[] BubbSprites;

        [TabGroup("游戏资源"), LabelText("舞台泡泡")]
        public GameObject StageBubble;

        [TabGroup("游戏资源"), LabelText("飞行中的球")]
        public GameObject FlyBubble;

#if UNITY_EDITOR

        [Button("保存", ButtonSizes.Medium)]
        private void Save()
        {
            AssetDatabase.SaveAssets();
        }
#endif
    }
}