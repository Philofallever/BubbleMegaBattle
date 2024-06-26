﻿using Logic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameUI
{
    public class StartPanel : Panel
    {
        [SerializeField, LabelText("开始游戏Btn")]
        private Button _startBtn;

        [SerializeField, LabelText("排行榜")]
        private Button _rankBtn;

        [SerializeField, LabelText("音乐")]
        private Button _musicBtn;

        private const string   _musicOn  = "音乐:开";
        private const string   _musicOff = "音乐:关";
        private       TMP_Text _musicState;

        private void Awake()
        {
            _musicState      = _musicBtn.GetComponentInChildren<TMP_Text>();
            _musicState.text = _musicOn;
            _startBtn.onClick.AddListener(OnStartClick);
            _rankBtn.onClick.AddListener(OnRankClick);
            _musicBtn.onClick.AddListener(OnMusicClick);
        }

        private void OnStartClick()
        {
            Manager.Instance.StartGame();
        }

        private void OnRankClick()
        {
            if(string.IsNullOrEmpty(Manager.Instance.PlayerName)) return;

            Manager.Instance.DisplayRecords();
        }

        private void OnMusicClick()
        {
            _musicState.text = _musicState.text == _musicOn ? _musicOff : _musicOn;
            Manager.Instance.ToggleBgm();
        }
    }
}