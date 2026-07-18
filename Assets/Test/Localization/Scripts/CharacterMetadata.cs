using UnityEngine.Localization.Metadata;

[System.Serializable]
[Metadata(AllowedTypes = MetadataType.StringTableEntry)]
public class CharacterMetadata : IMetadata
{
    public string Name;
    public string Description;
}
