using Config;
using Logic;
using UnityEngine;

namespace GamePrefab
{
    // 发射出去的泡泡
    public class FlyBubble : MonoBehaviour
    {
        private       SpriteRenderer _renderer;
        private       Rigidbody2D    _rigidbody;
        private const int            _fixedDeltaTimeScale = 10;

        public BubbType BubbType { get; private set; }

        private void Awake()
        {
            _renderer  = GetComponent<SpriteRenderer>();
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        public void Respawn(BubbType type, Vector2 flyDir, Vector3 pos)
        {
            Time.fixedDeltaTime  /= _fixedDeltaTimeScale;
            transform.position   =  new Vector3(pos.x, pos.y);
            _rigidbody.simulated =  true;
            _renderer.enabled    =  true;

            BubbType = type;
            var cfg = Manager.Instance.GameCfg;
            _renderer.sprite    = cfg.BubbSprites[(int) type];
            _rigidbody.velocity = flyDir * cfg.FlyBubbleSpeed;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            var involveBody = collision.rigidbody;
            // REMARK 碰到墙壁角,不精确??
            var notContactTop = collision.contacts[0].point.y + GameConstant.BubbRadius * 2 < Manager.Instance.StageAnchorData.TopEdge;
            //print($"{collision.contacts[0].point.x:F6} {collision.contacts[0].point.y:F6},{Manager.Instance.StageAnchorData.TopEdge:f6}");
            if (involveBody.bodyType == RigidbodyType2D.Static && notContactTop) //  碰到墙壁了
                return;

            if (_renderer.enabled == false) return; // 一个物理update碰撞到多个物体

            _renderer.enabled    =  false;
            _rigidbody.simulated =  false;
            Time.fixedDeltaTime  *= _fixedDeltaTimeScale;
            if (involveBody.bodyType == RigidbodyType2D.Kinematic)
                Manager.Instance.OnCollideStageBubble(collision);
            else
                Manager.Instance.OnCollideStageTopEdge(collision);
        }
    }
}