using UnityEngine;
using System.Collections;
using System.Text;
using GameUI;
using Logic;
using TMPro;
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
            _startPanel = Panel.LastShowPanelObj;
        base.OnEnable();
        var manager = Manager.Instance;
        var playerName = manager.PlayerName;
        _stringBuilder.AppendFormat(manager.GameCfg.RecordTitle,playerName);
        _stringBuilder.AppendLine();

        var records = manager.Records;
        foreach (var record in records)
        {
            print($"{record.Level} => {record.Score}");
            _stringBuilder.AppendFormat(manager.GameCfg.RecordEntry, record.Level+1, record.Score);
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