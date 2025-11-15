using SAS.Utilities;

public class PlayerSaveSystem : AutoInstantiateSingleton<PlayerSaveSystem>
{
    public PlayerSaveCollection playerSaveCollection;
    private static ISaveSystem _saveSystem;

    async protected override void Awake()
    {
        base.Awake();
        _saveSystem = new JsonFileSaveSystem(null);
        LoadPlayerData();
    }

    private async void LoadPlayerData()
    {
        playerSaveCollection = await _saveSystem.Load<PlayerSaveCollection>(0, "PlayerSaves", "PlayerSaveCollection")
            .ConfigureAwait(false);
        if (playerSaveCollection.PlayerSaveSlot.Count == 0)
            SaveDataGenerator.Generate(ref playerSaveCollection);
    }

    private void OnApplicationQuit()
    {
        _saveSystem.Save(0, "PlayerSaves", "PlayerSaveCollection", playerSaveCollection);
    }
}