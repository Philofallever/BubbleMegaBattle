using UnityEngine;

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
      , Green
      , Purple
      , Colorful
    }

    public static class BubbTypeUtil
    {
        public static BubbType GetRandomStageType()
        {
            return (BubbType) Random.Range((int) BubbType.Orange, (int) BubbType.Colorful);
        }
    }
}