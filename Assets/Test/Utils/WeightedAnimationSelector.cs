using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public struct WeightedAnimationOption
{
    [Tooltip("Value that will be written to the Animator integer parameter.")]
    public int value;
    [Min(0f)] public float weight;
}

public class WeightedAnimationSelector : StateMachineBehaviour
{
    [SerializeField] private string m_ParameterName = "AnimationVariant";
    [SerializeField] private WeightedAnimationOption[] m_Options;
    [SerializeField] private bool m_AvoidImmediateRepeat = true;

    private int _lastSelectedValue = int.MinValue;

    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        int selected = SelectWeightedRandom();

        if (selected == int.MinValue)
            return;

        animator.SetInteger(m_ParameterName, selected);
        _lastSelectedValue = selected;
    }

    private int SelectWeightedRandom()
    {
        if (m_Options == null || m_Options.Length == 0)
            return int.MinValue;

        float totalWeight = 0f;

        foreach (var option in m_Options)
        {
            if (option.weight <= 0f)
                continue;

            if (m_AvoidImmediateRepeat && m_Options.Length > 1 && option.value == _lastSelectedValue)
                continue;

            totalWeight += option.weight;
        }

        if (totalWeight <= 0f)
            return int.MinValue;

        float randomValue = Random.Range(0f, totalWeight);

        foreach (var option in m_Options)
        {
            if (option.weight <= 0f)
                continue;

            if (m_AvoidImmediateRepeat && m_Options.Length > 1 && option.value == _lastSelectedValue)
                continue;

            if (randomValue < option.weight)
                return option.value;

            randomValue -= option.weight;
        }

        return m_Options[^1].value;
    }
}