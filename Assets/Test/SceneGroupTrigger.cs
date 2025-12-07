using UnityEngine;

[RequireComponent(typeof(SceneGroupLoadConfig))]
public class SceneGroupTrigger : AllPlayersInTriggerHandler
{
    protected override void OnAllPlayersInside()
    {
        GetComponent<SceneGroupLoadConfig>().Load();
    }
}
