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
        }

        //[Button]
        //private void ShowBubble(StageType type)
        //{
        //    StageData.RebuildStage(type);
        //    foreach (var anchor in StageData)
        //    {
        //        var obj = Instantiate(Ball, anchor, Quaternion.identity);
        //        obj.GetComponent<SpriteRenderer>().color = Color.HSVToRGB(Random.value, Random.value, Random.value);
        //    }
        //}
    }
}