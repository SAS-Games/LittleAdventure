using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerSetupMenu : UIScreenView
{
    [SerializeField] private TextMeshProUGUI m_TitleText;
    [SerializeField] private GameObject m_ReadyText;
    [SerializeField] private UIButton m_ReadyButton;

    private float _ignoreInputTime = 1.5f;
    private bool _inputEnabled;
    private int _playerIndex;

    public void SetPlayerIndex(int pi)
    {
        _playerIndex = pi;
        m_TitleText.SetText("Player " + (pi + 1).ToString());
        _ignoreInputTime = Time.time + _ignoreInputTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > _ignoreInputTime)
        {
            _inputEnabled = true;
        }
    }

    public override void OnButtonClick(UIButton button, BaseEventData eventData)
    {
        if (button == m_ReadyButton)
        {
            m_ReadyText.SetActive(true);
            button.interactable = false;
            button.gameObject.SetActive(false);
            (this._parentUIScreenView as PlayerConfigurationUI).OnPlayerReady(_playerIndex);
        }
    }
}