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

    public class StageNode : IEnumerable<StageNode>
    {
        public int       Row;
        public int       Col;
        public Vector2   AnchorPos;
        public BubbType  BubbType;
        public StageNode ParentNode;

        // 左上相邻
        public StageNode GetUpLeft()
        {
            if (Row == 0) return null;

            var nodes    = Manager.Instance.StageNodeData;
            var anchors  = Manager.Instance.StageAnchorData;
            var rowCount = anchors.GetRowAnchorsCount(Row);

            if (rowCount == GameConstant.RowBubbMaxNum)
                return Col == 0 ? null : nodes[Row - 1][Col - 1];
            else
                return nodes[Row - 1][Col];
        }

        public StageNode GetUpRight()
        {
            if (Row == 0) return null;

            var nodes    = Manager.Instance.StageNodeData;
            var anchors  = Manager.Instance.StageAnchorData;
            var rowCount = anchors.GetRowAnchorsCount(Row);

            if (rowCount == GameConstant.RowBubbMaxNum)
                return Col == rowCount - 1 ? null : nodes[Row - 1][Col];
            else
                return nodes[Row - 1][Col + 1];
        }

        public StageNode GetLeft()
        {
            var nodes = Manager.Instance.StageNodeData;
            return Col == 0 ? null : nodes[Row][Col - 1];
        }

        public StageNode GetRight()
        {
            var nodes    = Manager.Instance.StageNodeData;
            var anchors  = Manager.Instance.StageAnchorData;
            var rowCount = anchors.GetRowAnchorsCount(Row);
            return Col == rowCount - 1 ? null : nodes[Row][Col + 1];
        }

        public StageNode GetDownLeft()
        {
            if (Row == GameConstant.StageRowCount - 1) return null;

            var nodes    = Manager.Instance.StageNodeData;
            var anchors  = Manager.Instance.StageAnchorData;
            var rowCount = anchors.GetRowAnchorsCount(Row);

            if (rowCount == GameConstant.RowBubbMaxNum)
                return Col == 0 ? null : nodes[Row + 1][Col - 1];
            else
                return nodes[Row + 1][Col];
        }

        public StageNode GetDownRight()
        {
            if (Row == GameConstant.StageRowCount - 1) return null;

            var nodes    = Manager.Instance.StageNodeData;
            var anchors  = Manager.Instance.StageAnchorData;
            var rowCount = anchors.GetRowAnchorsCount(Row);

            if (rowCount == GameConstant.RowBubbMaxNum)
                return Col == rowCount - 1 ? null : nodes[Row + 1][Col];
            else
                return nodes[Row + 1][Col + 1];
        }

        public IEnumerator<StageNode> GetEnumerator()
        {
            yield return GetUpLeft();
            yield return GetUpRight();
            yield return GetLeft();
            yield return GetRight();
            yield return GetDownLeft();
            yield return GetDownRight();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class StageAnchorData : IEnumerable<Vector2>
    {
        // 舞台边距,本应该计算得出,偷懒直接按照场景数据赋值
        public float     TopEdge       { get; private set; } = 8.6f;
        public float     BottomEdge    => TopEdge - GameConstant.RowHeight * GameConstant.StageRowCount;
        public float     LeftEdge      { get; private set; } = -5;
        public float     RightEdge     { get; private set; } = 5;
        public StageType CurrStageType { get; private set; }

        private readonly Vector2[][] _bubbleAnchors;

        // 迭代器实现
        private int _row;
        private int _col;

        public StageAnchorData(StageType stageType)
        {
            _bubbleAnchors = new Vector2[GameConstant.StageRowCount][];
            for (var i = 0; i < _bubbleAnchors.Length; ++i)
                _bubbleAnchors[i] = new Vector2[GameConstant.RowBubbMaxNum];

            RebuildStage(stageType);
        }

        // 构建舞台泡泡锚点
        public void RebuildStage(StageType type)
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

        //public Vector2Int CalcMostCloseAnchorIndex(Vector2 pos)
        //{
        //    var row = -1;
        //    for (var i = 0; i < GameConstant.StageRowCount; ++i)
        //    {
        //        var posibleY = TopEdge - GameConstant.BubbRadius - i * GameConstant.RowHeight;
        //        if (Mathf.Abs(posibleY - pos.y) <= GameConstant.RowHeight / 2)
        //        {
        //            row = i;
        //            break;
        //        }
        //    }

        //    if (row == -1)
        //    {
        //        Debug.Log($"未能找到最近的行,pos:{pos}");
        //        return Vector2Int.zero;
        //    }

        //    var count      = GetRowAnchorsCount(row);
        //    var rowAnchors = _bubbleAnchors[row];
        //    for (var i = 0; i < rowAnchors.Length; ++i)
        //    {
        //        if (Mathf.Abs(rowAnchors[i].y - pos.y) <= GameConstant.BubbRadius)
        //            return new Vector2Int(row, i);
        //    }

        //    Debug.Log($"未能找到最近的列,pos:{pos}");
        //    return Vector2Int.zero;
        //}

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
                for (var j = 0; j < rowAnchorCount; j++)
                {
                    yield return _bubbleAnchors[i][j];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}