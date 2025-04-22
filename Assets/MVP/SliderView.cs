using System;
using SAS.Utilities.TagSystem;
using UnityEngine.UI;

public class SliderView : ProxyViewBinder<float>
{
    [FieldRequiresChild] private Slider _sliderSlider;
    private void Awake()
    {
        this.Initialize();
    }

    protected override void OnValueChanged(float value)
    {
        _sliderSlider.value = value;
    }
}