using UnityEngine;
using System.Collections;
using System.Globalization;
using System.Threading;
using Config;
using Sirenix.OdinInspector;

namespace Logic
{
    public class Logic : MonoBehaviour
    {
        public        GameObject Ball;
        public static Logic      Instance  { get; private set; }
        public        StageData  StageData { get; private set; }

        //[RuntimeInitializeOnLoadMethod]
        //public static void Initialize()
        //{
        //    Application.targetFrameRate         = 60;
        //    Thread.CurrentThread.CurrentCulture = new CultureInfo("zh-CN");
        //    var obj = new GameObject(nameof(Logic)) {hideFlags = HideFlags.HideAndDontSave};
        //    Instance = obj.AddComponent<Logic>();
        //}

        protected void Awake()
        {
            StageData = new StageData();
            for (var i = 0; i < GameConstant.StageRowCount; i++)
            {
                var anchorsCount = StageData.GetRowBubleCount(i);
                for (var j = 0; j < anchorsCount; j++)
                {
                    var obj = Instantiate(Ball, StageData[i, j], Quaternion.identity);
                    obj.GetComponent<SpriteRenderer>().color = Mathf.CorrelatedColorTemperatureToRGB(Random.Range(1000, 4000));
                }
            }
        }

        protected void Update()
        {
        }
    }
}