using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class PlayerSetupMenu : UIScreenView
{
    [Header("UI")] [SerializeField] private TextMeshProUGUI m_TitleText;
    [SerializeField] private GameObject m_ReadyText;
    [SerializeField] private UIButton m_ReadyButton;
    [SerializeField] private TMP_Dropdown m_NameDropdown;
    [SerializeField] private TMP_Dropdown m_ColorDropdown;

    private float _ignoreInputTime = 1.5f;
    private bool _inputEnabled;
    private int _playerIndex;

    public Action<int, string> OnNameSelected;
    public Action<int, string> OnColorSelected;
    public Action<int> OnPlayerMarkedReady;

    private List<string> _nameOptions;
    private List<string> _colorOptions;

    protected override void Awake()
    {
        base.Awake();
        GetComponentInChildren<MultiplayerEventSystem>().SetSelectedGameObject(m_NameDropdown.gameObject);
        m_NameDropdown.onValueChanged.AddListener(_ => NotifyNameChange());
        m_ColorDropdown.onValueChanged.AddListener(_ => NotifyColorChange());
    }

    public void SetPlayerIndex(int index)
    {
        _playerIndex = index;
        m_TitleText.text = $"{_nameOptions[m_NameDropdown.value]}";
        _ignoreInputTime = Time.time + _ignoreInputTime;
    }

    public void SetNameOptions(List<string> names, string selectedName)
    {
        _nameOptions = names;
        SetupDropdown(m_NameDropdown, names, selectedName, _ => NotifyNameChange());
    }

    public void SetColorOptions(List<string> colors, string selectedColor)
    {
        _colorOptions = colors;
        SetupDropdown(m_ColorDropdown, colors, selectedColor, _ => NotifyColorChange());
    }

    private void SetupDropdown(TMP_Dropdown dropdown, List<string> options, string selectedValue,
        UnityAction<int> onChangeCallback)
    {
        dropdown.onValueChanged.RemoveAllListeners();
        dropdown.ClearOptions();
        dropdown.AddOptions(options);

        int selectedIndex = options.IndexOf(selectedValue);
        dropdown.value = selectedIndex >= 0 ? selectedIndex : 0;
        dropdown.RefreshShownValue();

        dropdown.onValueChanged.AddListener(onChangeCallback);
    }


    void Update()
    {
        if (Time.time > _ignoreInputTime)
            _inputEnabled = true;
    }

    public override void OnButtonClick(UIButton button, BaseEventData eventData)
    {
        if (!_inputEnabled)
            return;

        if (button == m_ReadyButton)
        {
            m_ReadyText.SetActive(true);
            GetComponent<CanvasGroup>().interactable = false;
            OnPlayerMarkedReady?.Invoke(_playerIndex);
        }
    }

    private void NotifyNameChange()
    {
        if (_nameOptions != null && m_NameDropdown.value >= 0 && m_NameDropdown.value < _nameOptions.Count)
        {
            m_TitleText.text = $"{_nameOptions[m_NameDropdown.value]}";
            OnNameSelected?.Invoke(_playerIndex, _nameOptions[m_NameDropdown.value]);
        }
    }

    private void NotifyColorChange()
    {
        if (_colorOptions != null && m_ColorDropdown.value >= 0 && m_ColorDropdown.value < _colorOptions.Count)
            OnColorSelected?.Invoke(_playerIndex, _colorOptions[m_ColorDropdown.value]);
    }
}