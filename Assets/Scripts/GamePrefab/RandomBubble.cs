using System;
using UnityEngine;
using System.Collections;
using Config;
using DG.Tweening;
using Logic;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace GamePrefab
{
    // 下一个随机出来的泡泡,决定待发射泡泡的颜色
    public class RandomBubble : MonoBehaviour
    {
        private Image       _img;
        private DOTweenPath _movePathAnim;

        public BubbType BubbType { get; private set; }

        public void Awake()
        {
            _img          = GetComponent<Image>();
            _movePathAnim = GetComponent<DOTweenPath>();
            _movePathAnim.onComplete.AddListener(OnAnimEnd);

            void OnAnimEnd()
            {
                gameObject.SetActive(false);
                _movePathAnim.DORewind();
            }
        }

        public void Respawn()
        {
            gameObject.SetActive(true);
            BubbType = (BubbType)UnityEngine.Random.Range(1, Enum.GetValues(typeof(BubbType)).Length);
            //BubbType = BubbType.Colorful;
            var cfg = Manager.Instance.GameCfg;
            _img.sprite = cfg.BubbSprites[(int) BubbType];
        }

        public void PlayMoveAnim(out float duration)
        {
            _movePathAnim.DORestart();
            duration = _movePathAnim.duration;
        }
    }
}