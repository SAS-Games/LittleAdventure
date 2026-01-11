using SAS.Core.TagSystem;

public class SliderViewBinder : RangeProxyViewBinder<float>
{
    [FieldRequiresChild] private SliderView _sliderView;
    private void Awake()
    {
        this.Initialize();
    }
    

    protected override void OnValueChanged(float current, float max)
    {
        _sliderView.value = current;
        _sliderView.SetMaxValue(max);
    }
}