using System;

namespace LevelStreaming.Editor
{
    /// <summary>
    /// Editor-side bridge populated by the optional Addressables editor assembly.
    /// </summary>
    public static class AddressablesEditorValidation
    {
        private static Func<string, string> s_Validator;
        private static Action<string> s_AssetAssigned;

        public static bool IsAvailable => s_Validator != null;

        public static void Register(Func<string, string> validator, Action<string> assetAssigned = null)
        {
            s_Validator = validator ?? throw new ArgumentNullException(nameof(validator));
            s_AssetAssigned = assetAssigned;
        }

        public static void NotifyAssetAssigned(string assetGuid)
        {
            if (!string.IsNullOrWhiteSpace(assetGuid))
                s_AssetAssigned?.Invoke(assetGuid);
        }

        public static bool TryValidate(string assetGuid, out string error)
        {
            if (s_Validator == null)
            {
                error = "requires Addressables, but its optional integration is not installed.";
                return false;
            }

            error = s_Validator(assetGuid);
            return string.IsNullOrWhiteSpace(error);
        }
    }
}
