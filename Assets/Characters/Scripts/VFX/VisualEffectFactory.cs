using SAS.Pool;
using UnityEngine;
using UnityEngine.VFX;

[CreateAssetMenu(menuName = "SAS/Pool/VisualEffect Factory")]
public class VisualEffectFactory : ScriptableObject, IFactory<VisualEffect>
{
    [SerializeField] private VisualEffect m_Prefab;
    public bool Create(out VisualEffect item)
    {
        item = GameObject.Instantiate(m_Prefab);
        return item != null;
    }

}
