using System;
using UnityEngine;
using System.Collections;
using Config;
using DG.Tweening;
using Logic;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace GameUI
{
    // 等待发射的泡泡,响应玩家UI操作,决定飞行方向
    public class WaitingBubble : MonoBehaviour
                               , IBeginDragHandler
                               , IDragHandler
                               , IEndDragHandler
    {
        [SerializeField, LabelText("发射线")]
        private LineRenderer _lineRenderer;

        [SerializeField, LabelText("拖动泡泡")]
        private Image _dragBubb;

        private const float     _punchDuration     = 0.2f;
        private const float     _lineMaterialScale = 11.8f;
        private       Tweener   _punchAnim;
        private       Image     _image;
        private       GamePanel _gamePanel;
        public        BubbType  BubbType     { get; private set; }
        public        Vector2   FlyDirection { get; private set; }

        private void Awake()
        {
            _image = GetComponent<Image>();
            _lineRenderer.material.SetTextureScale("_MainTex", new Vector2(_lineMaterialScale, 1f));
            _punchAnim = _image.rectTransform.DOPunchAnchorPos(Vector2.down * 20, _punchDuration, 5, 0).SetRelative(true).Pause().SetAutoKill(false);
            _gamePanel = GetComponentInParent<GamePanel>();
        }

        public YieldInstruction Respawn(BubbType type)
        {
            gameObject.SetActive(true);
            BubbType      = type;
            _image.sprite = Manager.Instance.GameCfg.BubbSprites[(int) BubbType];
            _punchAnim.Restart();
            return _punchAnim.WaitForCompletion();
        }

        #region 拖动操作相关

        public void OnBeginDrag(PointerEventData eventData)
        {
            var startPos = Vector3.Scale(transform.position, new Vector3(1, 1, 0));
            _lineRenderer.positionCount = 1;
            _lineRenderer.SetPosition(0, startPos);
            _dragBubb.sprite = _image.sprite;
            _dragBubb.gameObject.SetActive(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            var panelRect = (RectTransform) transform.parent;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRect, eventData.position, eventData.pressEventCamera, out var pos);
            var selfPos = ((RectTransform) transform).anchoredPosition;
            if (pos.y > selfPos.y) return;

            _dragBubb.rectTransform.anchoredPosition = pos;
            var ray             = Physics2D.Raycast(transform.position, (selfPos - pos));
            var stageAnchorData = Manager.Instance.StageAnchorData;

            if (ray.point.y < stageAnchorData.BottomEdge) return; // 不能出下边界

            FlyDirection                = (selfPos - pos).normalized;
            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPosition(1, ray.point);
            // 碰到墙壁
            if (ray.rigidbody.bodyType == RigidbodyType2D.Static && ray.point.y < stageAnchorData.TopEdge)
            {
                var offestY  = GameConstant.BubbRadius * (FlyDirection.y / FlyDirection.x); // tanθ = y/x
                var rayPoint = new Vector2(ray.point.x - Mathf.Sign(offestY) * GameConstant.BubbRadius, ray.point.y - Mathf.Abs(offestY));
                _lineRenderer.SetPosition(1, rayPoint);

                var rayDir = new Vector2(-FlyDirection.x, FlyDirection.y);
                ray      = Physics2D.Raycast(rayPoint, rayDir);
                rayPoint = ray.point;
                if (ray.rigidbody.bodyType == RigidbodyType2D.Static)
                    rayPoint = new Vector2(ray.point.x - Mathf.Sign(rayDir.x) * GameConstant.BubbRadius, ray.point.y - Mathf.Abs(offestY));

                _lineRenderer.positionCount = 3;
                _lineRenderer.SetPosition(2, rayPoint);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            gameObject.SetActive(false);
            _dragBubb.gameObject.SetActive(false);
            _lineRenderer.positionCount = 0;
            Manager.Instance.SpawnFlyBubble(BubbType, FlyDirection, transform.position);
        }

        #endregion
    }
}