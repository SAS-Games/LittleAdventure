using LevelStreaming.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace LevelStreaming.AddressablesIntegration.Editor
{
    [InitializeOnLoad]
    internal static class AddressablesEditorRegistration
    {
        static AddressablesEditorRegistration()
        {
            AddressablesEditorValidation.Register(ValidateAssetGuid, MakeAddressable);
        }

        private static string ValidateAssetGuid(string assetGuid)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.SettingsExists
                ? AddressableAssetSettingsDefaultObject.Settings
                : null;
            if (settings == null)
                return "uses Addressables, but no AddressableAssetSettings exist.";

            if (settings.FindAssetEntry(assetGuid, true) == null)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetGuid);
                return $"asset '{path}' is not in an Addressables group.";
            }

            return null;
        }

        private static void MakeAddressable(string assetGuid)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            if (settings == null || settings.FindAssetEntry(assetGuid, true) != null)
                return;

            settings.CreateOrMoveEntry(assetGuid, settings.DefaultGroup);
            EditorUtility.SetDirty(settings);
        }
    }
}
