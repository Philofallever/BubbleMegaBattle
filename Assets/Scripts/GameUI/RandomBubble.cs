using System;
using UnityEngine;
using System.Collections;
using Config;
using DG.Tweening;
using Logic;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace GameUI
{
    // 下一个随机出来的泡泡,决定待发射泡泡的颜色
    public class RandomBubble : MonoBehaviour
    {
        private Image       _img;
        private DOTweenPath _movePathAnim;
        private Vector3     _defaultPos;

        public BubbType BubbType { get; private set; }

        public void Awake()
        {
            _img          = GetComponent<Image>();
            _movePathAnim = GetComponent<DOTweenPath>();
            _defaultPos   = transform.localPosition;
            _movePathAnim.onComplete.AddListener(OnAnimEnd);

            void OnAnimEnd()
            {
                transform.localPosition = _defaultPos;
                gameObject.SetActive(false);
            }
        }

        public void Respawn()
        {
            gameObject.SetActive(true);
            var lvl    = Manager.Instance.Level;
            var weight = Manager.Instance.GameCfg.LevelTunnings[lvl].WaitBubbWeights;
            BubbType = weight.SelectByWeight();
            //BubbType = BubbType.Colorful;
            var cfg = Manager.Instance.GameCfg;
            _img.sprite = cfg.BubbSprites[(int) BubbType];
        }

        public YieldInstruction PlayMoveAnim()
        {
            _movePathAnim.tween.Restart();
            return _movePathAnim.tween.WaitForCompletion();
        }
    }
}