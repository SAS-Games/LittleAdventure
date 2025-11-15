using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class ItemData
{
    public string ClassType = "ItemData";
    public string ItemId;
    public int Qty;
    public string Metadata;
}

[Serializable]
public class PlayerData
{
    public string ClassType = "PlayerData";
    public string Name;
    public int Level;
    public int Health;
    public int XP;
    public List<ItemData> Inventory = new List<ItemData>();
}

[Serializable]
public class WorldState
{
    public string ClassType = "WorldState";
    public int Timestamp;
    public List<int> UnlockedLevels = new List<int>();
    public Dictionary<string, bool> Flags = new Dictionary<string, bool>();
}

[Serializable]
public class PlayerSaveSlot
{
    public int SlotId;
    public PlayerData PlayerData;
    public WorldState WorldState;
}

[Serializable]
public class PlayerSaveCollection
{
    public List<PlayerSaveSlot> PlayerSaveSlot = new List<PlayerSaveSlot>();
}

public static class SaveDataGenerator
{
    public static void Generate(ref PlayerSaveCollection playerSaveCollection, int slots = 50, int itemsPerSlot = 200)
    {
        System.Random rand = new System.Random();

        for (int s = 0; s < slots; s++)
        {
            PlayerSaveSlot slot = new PlayerSaveSlot();
            slot.SlotId = s;

            // Player data
            PlayerData p = new PlayerData();
            p.Name = RandomString(rand, 12);
            p.Level = rand.Next(1, 100);
            p.Health = rand.Next(50, 500);
            p.XP = rand.Next(0, 1_000_000);

            for (int i = 0; i < itemsPerSlot; i++)
            {
                p.Inventory.Add(new ItemData()
                {
                    ItemId = RandomString(rand, 8),
                    Qty = rand.Next(1, 50),
                    Metadata = RandomString(rand, 32)
                });
            }

            // World data
            WorldState w = new WorldState();
            w.Timestamp = rand.Next(100000, 999999);

            for (int j = 0; j < 100; j++)
                w.UnlockedLevels.Add(rand.Next(1, 200));

            for (int j = 0; j < 200; j++)
                w.Flags.Add(RandomString(rand, 6), rand.Next(0, 2) == 0);

            slot.PlayerData = p;
            slot.WorldState = w;

            playerSaveCollection.PlayerSaveSlot.Add(slot);
        }
    }

    // --------- HELPERS ---------
    private static string RandomString(System.Random rand, int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        char[] buffer = new char[length];
        for (int i = 0; i < length; i++)
            buffer[i] = chars[rand.Next(chars.Length)];
        return new string(buffer);
    }
}