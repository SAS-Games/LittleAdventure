using UnityEngine;

public class TestSinglePlayer : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("=====================Awake");
    }

    private void OnEnable()
    {
        Debug.Log("=====================OnEnable");
    }


    //void Start()
    //{
    //    if (PlayerSetupController.Instance)
    //    {
    //        PlayerSetupController.Instance.Clear();
    //        PlayerSetupController.Instance.AddDefaultPlayer();
    //    }
    //}
}
