using UnityEngine;

public class DisableObjectOnSceneLoad : MonoBehaviour
{
    private void Awake()
    {
        gameObject.SetActive(false);
    }
}
