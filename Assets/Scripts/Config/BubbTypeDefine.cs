using System;
using System.Collections.Generic;

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
            return (BubbType) UnityEngine.Random.Range((int) BubbType.Orange, (int) BubbType.Colorful);
        }

        public static BubbType GetRandomRandType()
        {
            return (BubbType) UnityEngine.Random.Range((int) BubbType.Orange, (int) BubbType.Colorful + 1);
        }

        // 按权重选取
        public static T SelectByWeight<T>(this IDictionary<T, int> dict)
        {
            var allWeight = 0;
            foreach (var itemWeight in dict.Values)
                allWeight += itemWeight;

            if (allWeight == 0) return default;

            var value = UnityEngine.Random.Range(0, allWeight);
            foreach (var item in dict)
            {
                if (value < item.Value)
                    return item.Key;

                value -= item.Value;
            }

            return default;
        }
    }
}