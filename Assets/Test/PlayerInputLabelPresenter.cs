using SAS.StateMachineCharacterController;
using SAS.Core.TagSystem;
using SAS.StringTest;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputLabelPresenter : MonoBehaviour
{
    [FieldRequiresSelf] private TMP_Text _tmp_Text;
    [SerializeField] private ReferenceString m_TextKey;
    [SerializeField] private DeviceInputLabelMap m_DeviceInputLabelMap;
    private string _text;
    private void Awake()
    {
        this.Initialize();

        _text = _tmp_Text.text;
    }

    public void UpdateControlLabel(GameObject gameObject)
    {
        var inputHandler = gameObject.GetComponent<IInputHandler>();
        if (inputHandler == null || inputHandler.PlayerInput == null)
            return;
        UpdateControlLabel(inputHandler.PlayerInput);
    }

    public void UpdateControlLabel(PlayerInput playerInput)
    {
        var label = m_DeviceInputLabelMap.GetLabel(playerInput, m_TextKey);
        string updatedText = _text.Replace("{Value}", label);
        _tmp_Text.text = updatedText;
    }
}
