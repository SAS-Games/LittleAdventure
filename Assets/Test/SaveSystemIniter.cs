using SAS.Core.TagSystem;
using UnityEngine;
using UserModel = DummyUserModel;
using SaveSystem = JsonFileSaveSystem;

[DefaultExecutionOrder(-90)]
public class SaveSystemIniter : MonoBehaviour
{
    [SerializeField] private BaseContextBinder m_ContextBinder;

    private void Awake()
    {
        IContextBinder context = m_ContextBinder;

        if (context == null)
        {
            Debug.LogError($"{nameof(SaveSystemIniter)} requires a context binder.", this);
            return;
        }

        IUserModel userModel = new UserModel();
        ISaveSystem saveSystem = new SaveSystem(Application.persistentDataPath);

        context.Add(typeof(IUserModel), userModel, default);
        context.Add(typeof(ISaveSystem), saveSystem, default);
    }
}
