using UnityEngine;
using System.Collections;
using Config;
using Logic;
using UnityEngine.UI;

namespace GamePrefab
{
    // 舞台上静止的泡泡
    public class StageBubble : MonoBehaviour
    {
        private SpriteRenderer _img;

        private void Awake()
        {
            _img = GetComponent<SpriteRenderer>();
        }

        public void SetBubbleType(BubbType bubbType)
        {
            _img.sprite = Manager.Instance.GameCfg.BubbSprites[(int) bubbType];
        }
    }
}