using UnityEngine;

public class TestSinglePlayer : MonoBehaviour
{
    void Start()
    {
        if (PlayerSetupController.Instance)
        {
            PlayerSetupController.Instance.Clear();
            PlayerSetupController.Instance.AddDefaultPlayer();
        }
    }
}
