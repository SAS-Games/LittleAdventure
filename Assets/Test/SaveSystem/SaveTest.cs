using System.Collections;
using UnityEngine;

public class SaveTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        while (PlayerSaveSystem.Instance.playerSaveCollection == null)
        {
            yield return null;
        }
        Debug.Log(PlayerSaveSystem.Instance.playerSaveCollection);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
