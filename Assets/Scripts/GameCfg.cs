using Logic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Config
{
    [TypeInfoBox("关卡难度设定")]
    public class LevelTunning
    {
        /* 原本是打算用代码生成的,想想自己能力算了= = 
         * 每个关卡应该有1-3个最终节点作为泡泡出生点,这种节点表示的是泡泡
         * 由上而下的情况下最终的收缩点,在生成时逆向而上,每个收缩点在上一行
         * 进行扩张,每行填充完成后,填充上一行,由此直至舞台填充完毕
         */

        //[LabelText("泡泡产生节点*"), Tooltip("按照行数递减填")]
        //public SpawnNode[] SpawnNodes;

        //[LabelText("左侧扩张几率"), Range(0, 1)]
        //public float ExpandLeft;

        //[LabelText("右侧扩张几率"), Range(0, 1)]
        //public float ExpandRight;

        //// 概率命中时必然同色,否则随机颜色,同色概率只能命中一个
        //[LabelText("下行左侧同色几率"), Range(0, 1)]
        //public float LowLeftSameRatio;

        //[LabelText("下行右侧同色几率"), Range(0, 1)]
        //public float LowRightSameRatio;

        //[LabelText("同行同色几率"), Range(0, 1)]
        //public float RowSameRatio;

        [LabelText("舞台样式")]
        public StageType StageType;

        [LabelText("初始填充泡泡")]
        public BubbType[][] InitBubles;

        [LabelText("下移需要发射次数")]
        public int MoveDownFlyTimes;
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