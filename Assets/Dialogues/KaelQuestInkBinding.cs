using System.Collections.Generic;
using SAS.DialogueSystem;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class KaelQuestInkBinding : InkStoryBinding
{
    private enum QuestState
    {
        Unavailable = 0,
        Active = 1,
        Completed = 2,
        Failed = 3
    }

    [Header("Story Variables")]
    [InkVariable("kael_requested_item")]
    [SerializeField] private string m_RequestedItemName = "Moonbloom Oil";

    [Header("Quest State")]
    [SerializeField] private QuestState m_QuestState = QuestState.Active;
    [SerializeField] private bool m_HasMetKael;
    [SerializeField] private bool m_QuestTimerExpired;
    [SerializeField, Range(0, 100)] private int m_ExcellentHealthThreshold = 75;
    [SerializeField, Range(0, 100)] private int m_UsableHealthThreshold = 25;
    [SerializeField] private int m_FullReward = 250;
    [SerializeField] private int m_ReducedReward = 100;
    [SerializeField] private List<int> m_MoonbloomOilHealths = new();
    [SerializeField] private int m_PlayerCoins;

    private int _lastReward;

    [InkExternal("kael_quest_state")]
    private int GetQuestState() => (int)m_QuestState;

    [InkExternal("kael_is_first_conversation")]
    private bool IsFirstConversation() => !m_HasMetKael;

    [InkExternal("kael_has_moonbloom_oil")]
    private bool HasRequestedItem() => m_MoonbloomOilHealths.Count > 0;

    [InkExternal("kael_mark_conversation_started")]
    private void MarkConversationStarted() => m_HasMetKael = true;

    [InkExternal("kael_last_reward")]
    private int GetLastReward() => _lastReward;

    // Returns: 0 item missing, 1 clean success, 2 damaged success,
    // 3 timer expired, 4 item unusable.
    [InkExternal("kael_try_deliver")]
    private int TryDeliverRequestedItem()
    {
        _lastReward = 0;
        if (m_QuestState != QuestState.Active || m_MoonbloomOilHealths.Count == 0)
            return 0;

        var selectedHealth = 0;
        foreach (var health in m_MoonbloomOilHealths)
            selectedHealth = Mathf.Max(selectedHealth, health);

        if (m_QuestTimerExpired)
        {
            m_QuestState = QuestState.Failed;
            return 3;
        }

        if (selectedHealth < m_UsableHealthThreshold)
        {
            m_QuestState = QuestState.Failed;
            return 4;
        }

        m_MoonbloomOilHealths.Remove(selectedHealth);
        _lastReward = selectedHealth >= m_ExcellentHealthThreshold ? m_FullReward : m_ReducedReward;
        m_PlayerCoins += _lastReward;
        m_QuestState = QuestState.Completed;
        return selectedHealth >= m_ExcellentHealthThreshold ? 1 : 2;
    }

    [ContextMenu("Add Excellent Moonbloom Oil")]
    private void AddExcellentOil() => m_MoonbloomOilHealths.Add(100);

    [ContextMenu("Add Damaged Moonbloom Oil")]
    private void AddDamagedOil() => m_MoonbloomOilHealths.Add(50);

    [ContextMenu("Add Unusable Moonbloom Oil")]
    private void AddUnusableOil() => m_MoonbloomOilHealths.Add(10);

    [ContextMenu("Reset Quest Demo State")]
    private void ResetState()
    {
        m_QuestState = QuestState.Active;
        m_HasMetKael = false;
        m_QuestTimerExpired = false;
        m_MoonbloomOilHealths.Clear();
        _lastReward = 0;
    }
}
