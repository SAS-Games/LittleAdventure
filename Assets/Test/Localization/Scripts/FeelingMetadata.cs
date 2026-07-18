using UnityEngine;
using UnityEngine.Localization.Metadata;

[System.Serializable]
[Metadata(AllowedTypes = MetadataType.StringTable)]
public class FeelingMetadata : IMetadata
{
    [TextArea]
    public string feeling;
}
