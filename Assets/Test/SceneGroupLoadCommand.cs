using SAS.SceneManagement;
using SAS.Core.TagSystem;
using UnityEngine;

namespace SAS.Utilities.DeveloperConsole
{
    [CreateAssetMenu(fileName = "New SceneGroupLoadCommand", menuName = "LittleAdventure/DeveloperConsole/Commands/SceneGroupLoadCommand")]
    public class SceneGroupLoadCommand : ConsoleCommand
    {
        [Inject] private ISceneLoader _sceneLoader;
        public override string HelpText => "Usage: LoadSceneGroup [group name]. Load the scene group with the given group name.";

        public override bool Process(DeveloperConsoleBehaviour developerConsole, string command, string[] args)
        {
            if (args != null && args.Length > 0)
            {
                bool loadOptionalScenes = false;
                if (args.Length > 1)
                {
                    if (!string.IsNullOrEmpty(args[1]) && !bool.TryParse(args[1], out loadOptionalScenes))
                        return false;
                }
                if(_sceneLoader == null)
                    developerConsole.InjectFieldBindings(this);
                _sceneLoader.LoadSceneGroupAsync(args[0], loadOptionalScenes);
                return true;
            }

            return false;
        }
    }
}