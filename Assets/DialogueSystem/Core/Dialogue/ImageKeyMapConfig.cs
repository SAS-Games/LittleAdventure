using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "ImageKeyMapConfig")]
public class ImageKeyMapConfig : ScriptableObject
{
    [System.Serializable]
    class ImageKeyMap
    {
        public string key;
        public Sprite value;
    }

    [SerializeField] private ImageKeyMap[] m_ImageKeyMap;

    public Sprite GetImage(string key)
    {
        var result = m_ImageKeyMap.FirstOrDefault(val => val.key == key);
        if (result == null)
        {
            Debug.LogWarning($"No entry found against the key: {key}");
            return null;
        }
        return result.value;
    }

}
