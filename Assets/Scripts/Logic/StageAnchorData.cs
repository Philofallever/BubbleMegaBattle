using System.Collections;
using System.Collections.Generic;
using Config;
using UnityEngine;

namespace Logic
{
    public enum StageType
    {
        EvenStage // 首行是偶数的舞台
      , OddStage  // 首行是奇数的舞台
    }

    // 泡泡锚点数据生成类
    public class StageAnchorData : IEnumerable<Vector2>
    {
        // 舞台边距,本应该计算得出,偷懒直接按照场景数据赋值
        public float     TopEdge       { get; private set; } = 8.6f;
        public float     BottomEdge    => TopEdge - GameConstant.RowHeight * GameConstant.StageRowCount;
        public float     LeftEdge      { get; private set; } = -5;
        public float     RightEdge     { get; private set; } = 5;
        public StageType CurrStageType { get; private set; }

        private readonly Vector2[][] _bubbleAnchors;

        public StageAnchorData(StageType stageType)
        {
            _bubbleAnchors = new Vector2[GameConstant.StageRowCount][];
            for (var i = 0; i < _bubbleAnchors.Length; ++i)
                _bubbleAnchors[i] = new Vector2[GameConstant.RowBubbMaxNum];

            RebuildStage(stageType);
        }

        // 构建舞台泡泡锚点
        private void RebuildStage(StageType type)
        {
            CurrStageType = type;

            for (var i = 0; i < _bubbleAnchors.Length; ++i)
            {
                var anchorY = TopEdge - GameConstant.BubbRadius - i * GameConstant.RowHeight; // 此行泡泡锚点X

                var offsetY = 0f;
                switch (CurrStageType)
                {
                    case StageType.EvenStage:
                        offsetY = (i & 1) == 0 ? GameConstant.BubbRadius : 2 * GameConstant.BubbRadius;
                        break;
                    case StageType.OddStage:
                        offsetY = (i & 1) == 0 ? 2 * GameConstant.BubbRadius : GameConstant.BubbRadius;
                        break;
                }

                var anchorXStart = LeftEdge + offsetY; // 此行第一个泡泡的锚点Y
                var anchors      = _bubbleAnchors[i];
                var anchorsCount = GetRowAnchorsCount(i); // 用到的不是全部10个位置
                for (var j = 0; j < anchorsCount; ++j)
                {
                    var anchorX = anchorXStart + j * 2 * GameConstant.BubbRadius;
                    anchors[j].Set(anchorX, anchorY);
                }
            }
        }

        // 锚点数据中存在不合理的数据,提供接口给外部访问
        public Vector2 this[int row, int col]
        {
            get
            {
                if (row >= GameConstant.StageRowCount || row < 0)
                {
                    Debug.LogError($"StageAnchorData getter row:{row} 超出了行数");
                    return Vector2.negativeInfinity;
                }

                var bubleCount = GetRowAnchorsCount(row);
                if (col >= bubleCount || col < 0)
                {
                    Debug.LogError($"StageAnchorData getter col:{col} 超出了泡泡数");
                    return Vector2.negativeInfinity;
                }

                return _bubbleAnchors[row][col];
            }
        }

        public int GetRowAnchorsCount(int rowIndex)
        {
            // 舞台类型 => 奇偶行泡泡数
            var evenCount = CurrStageType == StageType.EvenStage ? GameConstant.RowBubbMaxNum : GameConstant.RowBubbMinNum;
            var oddCount  = CurrStageType == StageType.EvenStage ? GameConstant.RowBubbMinNum : GameConstant.RowBubbMaxNum;

            return (rowIndex & 1) == 0 ? evenCount : oddCount;
        }

        public IEnumerator<Vector2> GetEnumerator()
        {
            for (var i = 0; i < GameConstant.StageRowCount; i++)
            {
                var rowAnchorCount = GetRowAnchorsCount(i);
                for (var j = 0; j < rowAnchorCount; j++) yield return _bubbleAnchors[i][j];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}