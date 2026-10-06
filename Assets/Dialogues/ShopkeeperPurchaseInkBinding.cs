using SAS.DialogueSystem;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ShopkeeperPurchaseInkBinding : InkStoryBinding
{
    [Header("Story Variables")]
    [InkVariable("shop_item_name")]
    [SerializeField] private string m_ItemName = "Tempering Powder";
    [InkVariable("shop_item_price")]
    [SerializeField, Min(0)] private int m_ItemPrice = 120;

    [Header("Player State")]
    [SerializeField, Min(0)] private int m_Coins = 120;
    [SerializeField, Min(1)] private int m_InventorySlotCapacity = 4;
    [SerializeField, Min(0)] private int m_OccupiedInventorySlots;
    [SerializeField, Min(0)] private int m_ItemQuantity;
    [SerializeField, Min(1)] private int m_ItemStackLimit = 99;

    [InkExternal("shop_coin_count")]
    private int GetCoinCount() => m_Coins;

    [InkExternal("shop_item_quantity")]
    private int GetItemQuantity() => m_ItemQuantity;

    // Returns: 0 not enough Coins, 1 inventory cannot receive item, 2 success.
    [InkExternal("shop_try_purchase")]
    private int TryPurchase()
    {
        if (m_Coins < m_ItemPrice)
            return 0;

        var canStack = m_ItemQuantity > 0 && m_ItemQuantity < m_ItemStackLimit;
        var hasFreeSlot = m_OccupiedInventorySlots < m_InventorySlotCapacity;
        if (!canStack && !hasFreeSlot)
            return 1;

        // Commit both sides only after every validation succeeds.
        m_Coins -= m_ItemPrice;
        if (m_ItemQuantity == 0)
            m_OccupiedInventorySlots++;
        m_ItemQuantity++;
        return 2;
    }

    [ContextMenu("Add Purchase Price In Coins")]
    private void AddPurchasePrice() => m_Coins += m_ItemPrice;

    [ContextMenu("Toggle Inventory Full")]
    private void ToggleInventoryFull()
    {
        m_ItemQuantity = 0;
        m_OccupiedInventorySlots = m_OccupiedInventorySlots >= m_InventorySlotCapacity
            ? 0
            : m_InventorySlotCapacity;
    }

    [ContextMenu("Reset Shop Demo State")]
    private void ResetState()
    {
        m_Coins = m_ItemPrice;
        m_OccupiedInventorySlots = 0;
        m_ItemQuantity = 0;
    }
}
