using SAS.Utilities.TagSystem;
using UnityEngine;

public static class FlexPrefsConfig
{
    public const string FileName = "CoopFlexPrefsData";
}
public class CoopFlexPrefsIniter : MonoBehaviour
{
    [SerializeField] private int m_MaxPlayers = 2;
    [Inject] private ISaveSystem _flexPrefsSaveSystem;

    public async void Awake()
    {
        this.InjectFieldBindings();

        for (int i = 0; i < m_MaxPlayers; i++)
        {
            if (!FlexPrefs.IsUserDataLoaded(i, FlexPrefsConfig.FileName))
            {
                Debug.Log("[FlexPrefsBootstrapper] Preloading user data...");
                await FlexPrefs.PreloadUserAsync(i, FlexPrefsConfig.FileName);
            }
        }
    }
}