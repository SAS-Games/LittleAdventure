using SAS.Core.TagSystem;
using UnityEngine;
using UserModel = DummyUserModel;
using SaveSystem = JsonFileSaveSystem;

public class SaveSystemIniter : MonoBehaviour
{
   private IUserModel _userModel;
   private ISaveSystem _saveSystem;
   
  [SerializeField] private BaseContextBinder m_ContextBinder;
   void Awake()
   {
      _userModel = new UserModel();
      _saveSystem = new SaveSystem(Application.persistentDataPath);

      (m_ContextBinder as IContextBinder).Add(typeof(UserModel), _userModel, default);
      (m_ContextBinder as IContextBinder).Add(typeof(SaveSystem), _saveSystem, default);
   }
}
