using UnityEngine;
using System.Collections;
using Config;
using DG.Tweening;
using Logic;
using UnityEngine.UI;

namespace GamePrefab
{
    // 舞台上静止的泡泡
    public class StageBubble : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        private Tweener        _wipeAnim;
        public  StageNode      StageNode { get; private set; }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        public void Respawn(StageNode node)
        {
            StageNode        = node;
            _renderer.sprite = Manager.Instance.GameCfg.BubbSprites[(int) node.BubbType];
        }

        public void PlayWipeAnim()
        {
            if (_wipeAnim == null)
                _wipeAnim = transform.DOMove(Vector3.down * 2, 0.6f).SetRelative(true).Pause().OnComplete(() => Destroy(gameObject));
            _wipeAnim.Restart();
        }
    }
}