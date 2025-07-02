using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerSetupMenuController : MonoBehaviour
{
    private int playerIndex;

    [FormerlySerializedAs("titleText")] [SerializeField] private TextMeshProUGUI m_TitleText;
    [FormerlySerializedAs("readyPanel")] [SerializeField] private GameObject m_ReadyPanel;
    [FormerlySerializedAs("menuPanel")] [SerializeField] private GameObject m_MenuPanel;
    [FormerlySerializedAs("readyButton")] [SerializeField] private Button m_ReadyButton;

    private float ignoreInputTime = 1.5f;
    private bool inputEnabled;
    public void SetPlayerIndex(int pi)
    {
        playerIndex = pi;
        m_TitleText.SetText("Player " + (pi + 1).ToString());
        ignoreInputTime = Time.time + ignoreInputTime;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > ignoreInputTime)
        {
            inputEnabled = true;
        }
    }

    public void SelectColor(Material mat)
    {
        if(!inputEnabled) { return; }

        //PlayerConfigurationManager.Instance.SetPlayerColor(playerIndex, mat);
        m_ReadyPanel.SetActive(true);
        m_ReadyButton.interactable = true;
        m_MenuPanel.SetActive(false);
        m_ReadyButton.Select();
        
    }

    // public void ReadyPlayer()
    // {
    //     if (!inputEnabled) { return; }
    //
    //     PlayerSetupController.Instance.ReadyPlayer(playerIndex);
    //     m_ReadyButton.gameObject.SetActive(false);
    // }
}
