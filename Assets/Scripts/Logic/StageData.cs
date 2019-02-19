using Config;
using UnityEngine;

namespace Logic
{
    public enum StageType
    {
        EvenStage // 首行是偶数的舞台
      , OddStage  // 首行是奇数的舞台
    }

    public class StageData
    {
        public float     TopEdge       { get; private set; } = 9.6f;
        public float     LeftEdge      { get; private set; } = -5;
        public float     RightEdge     { get; private set; } = 5;
        public StageType CurrStageType { get; private set; }

        private readonly Vector2[][] _bubbleAnchors;

        public StageData()
        {
            _bubbleAnchors = new Vector2[GameConstant.StageRowCount][];
            for (var i = 0; i < _bubbleAnchors.Length; ++i)
                _bubbleAnchors[i] = new Vector2[GameConstant.RowBubbMaxNum];

            RebuildStage(StageType.EvenStage);
        }

        // 构建舞台泡泡锚点
        public void RebuildStage(StageType type)
        {
            CurrStageType = type;


            for (var i = 0; i < _bubbleAnchors.Length; ++i)
            {
                var anchorX = TopEdge - GameConstant.BubbRadius - i * GameConstant.RowHeight; // 此行泡泡锚点X

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

                var anchorYStart = LeftEdge + offsetY; // 此行第一个泡泡的锚点Y
                var anchors      = _bubbleAnchors[i];
                var anchorsCount = GetRowBubleCount(i); // 用到的不是全部10个位置
                for (var j = 0; j < anchorsCount; ++j)
                {
                    var anchorY = anchorYStart + j * 2 * GameConstant.BubbRadius;
                    anchors[j].Set(anchorX, anchorY);
                }
            }
        }

        // 锚点数据中存在不合理的数据,提供接口给外部访问
        public Vector2 this[int row, int col]
        {
            get
            {
                if (col >= GameConstant.StageRowCount || col < 0)
                {
                    Debug.LogError($"StageData getter Y:{col} 超出了行数");
                    return Vector2.negativeInfinity;
                }

                var bubleCount = GetRowBubleCount(row);
                if (row >= bubleCount || row < 0)
                {
                    Debug.LogError($"StageData getter X:{row} 超出了泡泡数");
                    return Vector2.negativeInfinity;
                }

                return _bubbleAnchors[row][col];
            }
        }

        public Vector2Int CalcMostCloseAnchorIndex(Vector2 pos)
        {
            var row = -1;
            for (var i = 0; i < GameConstant.StageRowCount; ++i)
            {
                var posibleY = TopEdge - GameConstant.BubbRadius - i * GameConstant.RowHeight;
                if (Mathf.Abs(posibleY - pos.y) <= GameConstant.RowHeight / 2)
                {
                    row = i;
                    break;
                }
            }

            if (row == -1)
            {
                Debug.Log($"未能找到最近的行,pos:{pos}");
                return Vector2Int.CeilToInt(pos);
            }

            var count      = GetRowBubleCount(row);
            var rowAnchors = _bubbleAnchors[row];
            for (var i = 0; i < rowAnchors.Length; ++i)
            {
                if (Mathf.Abs(rowAnchors[i].y - pos.y) <= GameConstant.BubbRadius)
                    return new Vector2Int(row, i);
            }

            Debug.Log($"未能找到最近的列,pos:{pos}");
            return Vector2Int.CeilToInt(pos);
        }

        public int GetRowBubleCount(int rowIndex)
        {
            // 舞台类型 => 奇偶行泡泡数
            var evenCount = CurrStageType == StageType.EvenStage ? GameConstant.RowBubbMaxNum : GameConstant.RowBubbMinNum;
            var oddCount  = CurrStageType == StageType.EvenStage ? GameConstant.RowBubbMinNum : GameConstant.RowBubbMaxNum;

            return (rowIndex & 1) == 0 ? evenCount : oddCount;
        }
    }
}