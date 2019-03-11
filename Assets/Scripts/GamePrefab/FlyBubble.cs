using System.CodeDom;
using UnityEngine;
using System.Collections;
using Config;
using Logic;


namespace GamePrefab
{
    // 发射出去的泡泡
    public class FlyBubble : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        private Rigidbody2D    _rigidbody;
        public  BubbType       BubbType { get; private set; }


        private void Awake()
        {
            _renderer  = GetComponent<SpriteRenderer>();
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        public void Respawn(BubbType type, Vector2 flyDir, Vector3 pos)
        {
            transform.position   = new Vector3(pos.x, pos.y);
            _rigidbody.simulated = true;
            _renderer.enabled    = true;

            BubbType = type;
            var cfg = Manager.Instance.GameCfg;
            _renderer.sprite    = cfg.BubbSprites[(int) type];
            _rigidbody.velocity = flyDir * cfg.FlyBubbleSpeed;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            var otherBody = collision.rigidbody;
            if (otherBody.bodyType == RigidbodyType2D.Static) //  碰到墙壁了
                return;

            _rigidbody.simulated = false;
            _renderer.enabled    = false;
            var point = collision.contacts[0];
            Manager.Instance.OnCollideStageBubble(point.point);
        }
    }
}