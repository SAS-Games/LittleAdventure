using SAS.Utilities.TagSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderView : Slider
{
    [FieldRequiresChild] private TMP_Text _tmpText;

    protected override void Awake()
    {
        this.Initialize();
        base.Awake();
        onValueChanged.AddListener(UpdateText);
        UpdateText(value);
    }

    public void SetMaxValue(float value)
    {
        if (maxValue == value)
            return;
        maxValue = value;
        onValueChanged.Invoke(value);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        onValueChanged.RemoveListener(UpdateText);
    }

    private void UpdateText(float val)
    {
        if (_tmpText != null)
            _tmpText.text = $"{Mathf.RoundToInt(val)} / {Mathf.RoundToInt(maxValue)}";
    }
}