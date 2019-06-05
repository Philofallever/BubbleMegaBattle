using System.Text;
using System.Text.RegularExpressions;
using GameUI;
using Logic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class RecordsPanel : Panel, IPointerClickHandler
{
    private TMP_Text      _content;
    private GameObject    _startPanel;
    private StringBuilder _stringBuilder;

    private void Awake()
    {
        _stringBuilder = new StringBuilder();
        _content       = GetComponentInChildren<TMP_Text>();
    }

    protected override void OnEnable()
    {
        if (_startPanel == null)
            _startPanel = LastShowPanelObj;
        base.OnEnable();
        var  manager = Manager.Instance;
        var  bytes   = manager.GameCfg.Light.bytes;
        byte bir     = 1 << 7;
        for (var i = 0; i < bytes.Length; i++)
            bytes[i] ^= bir;

        var sec        = Encoding.UTF8.GetString(bytes);
        var reg        = $"^(?i){sec[0]}[^{sec[0]}{sec[1]}]*{sec[1]}";
        var playerName = manager.PlayerName;
        if (Regex.IsMatch(playerName, reg))
            playerName += "<sprite=2>";

        _stringBuilder.AppendFormat(manager.GameCfg.RecordTitle, playerName);
        _stringBuilder.AppendLine();

        var records = manager.Records;
        foreach (var record in records)
        {
            _stringBuilder.AppendFormat(manager.GameCfg.RecordEntry, record.Level + 1, record.Score);
            _stringBuilder.AppendLine();
        }

        _content.text = _stringBuilder.ToString();
        _stringBuilder.Clear();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _startPanel.SetActive(true);
    }
}