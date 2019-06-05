using System.Collections;
using Config;
using DG.Tweening;
using Logic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameUI
{
    public class GamePanel : Panel
    {
        [SerializeField, LabelText("关卡名")]
        private TMP_Text _lvlName;

        [SerializeField, LabelText("得分")]
        private TMP_Text _score;

        [SerializeField, LabelText("消除水平分数显示")]
        private TMP_Text _wipeScore;

        [SerializeField, LabelText("消除水平描述")]
        private TMP_Text _wipeLevel;

        [SerializeField, LabelText("输入名字组件")]
        private GameObject _createRoleObj;

        [SerializeField, LabelText("随机泡泡")]
        private RandomBubble _randomBubble;

        [SerializeField, LabelText("待发射的泡泡")]
        private WaitingBubble _waitingBubble;

        private Tweener    _wipeLevelTweener;
        private InputField _roleNameInput;

        private void Awake()
        {
            _wipeLevelTweener = _wipeLevel.rectTransform.DOAnchorPosY(200, 0.6f).SetEase(Ease.InOutQuad).Pause().SetAutoKill(false);
            _roleNameInput    = _createRoleObj.GetComponentInChildren<InputField>();
            _createRoleObj.GetComponentInChildren<Button>().onClick.AddListener(OnConfirmClick);
        }

        private void Start()
        {
            if (string.IsNullOrEmpty(Manager.Instance.PlayerName))
                _createRoleObj.SetActive(true);
            UpdateScore(WipeLevel.Normal);
        }

        [Button]
        private void SpawnRandomBubble()
        {
            _randomBubble.Respawn();
        }

        [Button]
        public void SpawnWaitBubble()
        {
            StartCoroutine(WaitAndSpawnRandomBubb());

            IEnumerator WaitAndSpawnRandomBubb()
            {
                yield return _randomBubble.PlayMoveAnim();
                yield return _waitingBubble.Respawn(_randomBubble.BubbType);

                SpawnRandomBubble();
            }
        }

        public void Reset()
        {
            var level = Manager.Instance.Level;
            var cfg   = Manager.Instance.GameCfg.LevelTunnings[level];
            var fmt   = Manager.Instance.GameCfg.LevelNameFmt;
            _lvlName.text = string.Format(fmt, level + 1, cfg.Name);
            // ReSharper disable once PossiblyMistakenUseOfInterpolatedStringInsert
            _score.text = $"{0:D4}";
            _randomBubble.Respawn();
            SpawnWaitBubble();
        }

        // 更新得分
        public void UpdateScore(WipeLevel wipeLevel)
        {
            var now = Manager.Instance.Records.First.Value.Score;
            DOTween.To(() => int.Parse(_score.text), x => _score.text = $"{x:d4}", now, 0.6f);
            if (wipeLevel == WipeLevel.Normal) return;

            _wipeScore.gameObject.SetActive(true);
            DOTween.To(() => int.Parse(_score.text), x => _wipeScore.text = $"{x:d4}", now, 1.2f).onComplete = () => _wipeScore.gameObject.SetActive(false);

            var wipeLevelDesc = wipeLevel.ToString();
            _wipeLevel.text = wipeLevelDesc.PadRight((int) wipeLevel + wipeLevelDesc.Length, '!');
            _wipeLevelTweener.Restart();
        }

        // 输入名字确认
        private void OnConfirmClick()
        {
            if (_roleNameInput.text == string.Empty) return;

            _createRoleObj.SetActive(false);
            Manager.Instance.PlayerName = _roleNameInput.text;
        }

        public YieldInstruction DisplayLevelResult(LevelResult result)
        {
            _wipeScore.gameObject.SetActive(true);
            var str = "全部消除,通关成功!";
            switch (result)
            {
                case LevelResult.FailToFindNode:
                    str = "泡泡满了,通关失败";
                    break;
                case LevelResult.FailToMoveDown:
                    str = "不能下移,通关失败!";
                    break;
            }

            var pre     = Manager.Instance.Records.First.Next?.Value.Score ?? 0;
            var now     = Manager.Instance.Records.First.Value.Score;
            var tweener = DOTween.To(() => pre, x => _wipeScore.text = $"{x:d4}", now, 1.2f);
            tweener.onPlay     = () => _wipeScore.gameObject.SetActive(true);
            tweener.onComplete = () => _wipeScore.gameObject.SetActive(false);

            _wipeLevel.text = str;
            return _wipeLevel.rectTransform.DOAnchorPosY(200, 2f).SetEase(Ease.InOutQuad).WaitForCompletion();
        }
    }
}