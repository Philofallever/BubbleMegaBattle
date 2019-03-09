using System;
using UnityEngine;
using System.Collections;
using Config;
using DG.Tweening;
using Logic;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace GamePrefab
{
    // 等待发射的泡泡,响应玩家UI操作,决定飞行方向
    public class WaitingBubble : MonoBehaviour
                               , IBeginDragHandler
                               , IDragHandler
                               , IEndDragHandler
    {
        [SerializeField, LabelText("发射线")]
        private LineRenderer _lineRenderer;

        private const float _punchDuration = 0.2f;

        private Image    _image;
        public  BubbType BubbType { get; private set; }

        [SerializeField]
        private Vector2 _flyDirection;

        private Vector3[] _linePositions;


        private void Awake()
        {
            _image         = GetComponent<Image>();
            _linePositions = new Vector3[3];
        }


        public void Respawn(BubbType type, out float animDuration)
        {
            gameObject.SetActive(true);
            animDuration  = _punchDuration;
            BubbType      = type;
            _image.sprite = Manager.Instance.GameCfg.BubbSprites[(int) BubbType];
            _image.rectTransform.DOPunchAnchorPos(Vector2.down * 20, _punchDuration, 5, 0).SetRelative(true);
        }

        #region 拖动操作相关

        public void OnBeginDrag(PointerEventData eventData)
        {
            var startPos = Vector3.Scale(transform.position, new Vector3(1, 1, 0));
            _lineRenderer.positionCount = 1;
            _lineRenderer.SetPosition(0, startPos);
        }

        public void OnDrag(PointerEventData eventData)
        {
            var panelRect = (RectTransform) transform.parent;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRect, eventData.position, eventData.pressEventCamera, out var pos);
            var selfPos = ((RectTransform) transform).anchoredPosition;

            if (pos.y > selfPos.y) return;

            var ray = Physics2D.Raycast(transform.position, (selfPos - pos));
            if (ray.point.y < 9.6 - Constant.RowHeight * Constant.StageRowCount) return;

            _flyDirection               = (selfPos - pos).normalized;
            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPosition(1, ray.point);
            if (ray.rigidbody.bodyType == RigidbodyType2D.Static && ray.point.y < 9.6)
            {
                var rayDir = Vector2.Scale(_flyDirection, new Vector2(-1, 1));
                ray                         = Physics2D.Raycast(new Vector2(Mathf.Clamp(ray.point.x, -5, 5), ray.point.y), rayDir);
                _lineRenderer.positionCount = 3;
                print($"raydir = {rayDir} {ray.transform.name}");
                _lineRenderer.SetPosition(2, ray.point);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            //Manager.Instance.SpawnFlyBubble();
        }

        #endregion
    }
}