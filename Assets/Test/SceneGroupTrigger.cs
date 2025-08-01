using UnityEngine;

[RequireComponent(typeof(SceneGroupLoader))]
public class SceneGroupTrigger : AllPlayersInTriggerHandler
{
    protected override void OnAllPlayersInside()
    {
        GetComponent<SceneGroupLoader>().Load();
    }
}
