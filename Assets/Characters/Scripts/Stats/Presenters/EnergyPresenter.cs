using SAS.TimerSystem;
using SAS.Utilities.TagSystem;
using UnityEngine;

public class EnergyPresenter : StatPresenter<IEnergyModel>
{
    [field: FieldRequiresSelf(tag: Tag.Energy)] protected override IProxyView View { get; }
    [SerializeField] private float m_EnergyRegenAmount = 0.1f;
    [SerializeField] private int m_EnergyRegenFrequency = 1;
    private FrequencyTimer _regenTimer;

    protected override void Awake()
    {
        base.Awake();
        _regenTimer = new FrequencyTimer(m_EnergyRegenFrequency);
        _regenTimer.OnTick = RegenerateEnergy;
    }

    private void Start()
    {
        _regenTimer.Start();
    }

    private void RegenerateEnergy()
    {
        _model.Increase(m_EnergyRegenAmount);
    }

    void OnEnable()
    {
        if (!_regenTimer.IsFinished)
            _regenTimer.Resume();
    }

    void OnDisable()
    {
        if (!_regenTimer.IsFinished)
            _regenTimer.Pause();
    }
}