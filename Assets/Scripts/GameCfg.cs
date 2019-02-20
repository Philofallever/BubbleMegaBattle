using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

namespace Config
{
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

    [TypeInfoBox("关卡难度设定")]
    public class LevelTunning
    {
        [LabelText("泡泡行数")]
        public int GenRow;

        [LabelText("最少泡泡数")]
        public int GenBubbCount;

        [LabelText("同行同色几率"), Range(0, 1)]
        public float PreSameRatio;

        [LabelText("上行同色几率"), Range(0, 1)]
        public float PreRowSameRatio;

        [LabelText("下移需要发射次数")]
        public int MoveDownShotTimes;
    }

    public class GameCfg : SerializedScriptableObject
    {
        [LabelText("游戏规则"), Multiline(5)]
        public string RuleDesc;

        [LabelText("静止气泡")]
        public GameObject StaticBubble;

        [LabelText("飞行气泡")]
        public GameObject FlyBubble;

        [LabelText("额外奖励分")]
        public int ExtraGrade;
    }
}