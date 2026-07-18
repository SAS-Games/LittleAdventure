using UnityEditor;

[InitializeOnLoad]
public static class BattleArenaOpenWorldAutoBuild
{
    static BattleArenaOpenWorldAutoBuild()
    {
        // The blockout scene is generated on disk. Keep automatic rebuild disabled so
        // Unity does not overwrite the scene unexpectedly when scripts reload.
    }
}
