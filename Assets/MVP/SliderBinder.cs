using SAS.Utilities.TagSystem;
using UnityEngine.UI;

public class SliderBinder : ProxyViewBinder<float>
{
    [FieldRequiresChild] private Slider _slider;
    private void Awake()
    {
        this.Initialize();
    }

    protected override void OnValueChanged(float value)
    {
        _slider.value = value;
    }
}