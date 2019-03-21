using System.Collections;
using System.Collections.Generic;
using Config;
using UnityEngine;

namespace Logic
{
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
}