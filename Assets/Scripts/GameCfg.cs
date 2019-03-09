using UnityEngine;
using System.Collections;
using Logic;
using Sirenix.OdinInspector;
using UnityEditor;

namespace Config
{
    public enum BubbType
    {
        Empty
      , Orange
      , Pink
      , Red
      , Yellow
      , Blue
      , Greem
      , Purple
      , Colorful
    }


    public class Constant
    {
        public const           int   StageRowCount = 14; // 泡泡锚点行数
        public const           int   RowBubbMaxNum = 10;
        public const           int   RowBubbMinNum = RowBubbMaxNum - 1;
        public const           float BubbRadius    = 0.5f; // 泡泡半径(unity单位)
        public static readonly float RowHeight;

        static Constant()
        {
            RowHeight = 2 * BubbRadius * Mathf.Sin(Mathf.PI / 3);
        }
    }

    public struct SpawnNode
    {
        public Vector2Int AnchorPos;
        public BubbType   Type;
    }

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
        public int MoveDownShotTimes;
    }

    public class GameCfg : SerializedScriptableObject
    {
        [LabelText("游戏规则"), Multiline(5)]
        public string RuleDesc;

        [LabelText("泡泡精灵")]
        public Sprite[] BubbSprites;

        [LabelText("舞台气泡")]
        public GameObject StageBubble;

        [LabelText("飞行气泡")]
        public GameObject FlyBubble;

        [LabelText("额外奖励分")]
        public int ExtraGrade;

        [LabelText("关卡最大数*")]
        public int MaxLevel;

        [LabelText("关卡配置")]
        public LevelTunning[] LevelTunnings;

        [Button("保存", ButtonSizes.Medium)]
        private void Save()
        {
            AssetDatabase.SaveAssets();
        }
    }
}