using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteInEditMode, DisallowMultipleComponent]
public class ObjectGuid : MonoBehaviour, ISerializationCallbackReceiver
{
    private Guid _guid = Guid.Empty;

    // Unity's serialization system doesn't know about System.Guid, so we convert to a byte array
    [SerializeField] private byte[] serializedGuid;

    public bool IsGuidAssigned => _guid != Guid.Empty;

    void CreateGuid()
    {
        // if our serialized data is invalid, then we are a new object and need a new GUID
        if (serializedGuid == null || serializedGuid.Length != 16)
        {
#if UNITY_EDITOR
            // if in editor, make sure we aren't a prefab of some kind
            if (IsAssetOnDisk())
                return;
            Undo.RecordObject(this, "Added GUID");
#endif
            _guid = Guid.NewGuid();
            serializedGuid = _guid.ToByteArray();

#if UNITY_EDITOR
            // If we are creating a new GUID for a prefab instance of a prefab, but we have somehow lost our prefab connection
            // force a save of the modified prefab instance properties
            if (PrefabUtility.IsPartOfNonAssetPrefabInstance(this))
                PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif
        }
        else if (_guid == Guid.Empty)
            _guid = new Guid(serializedGuid);

        // register with the GUID Manager so that other components can access this
        if (_guid != Guid.Empty)
        {
            if (!GuidLookupTable.Add(this))
            {
                // if registration fails, we probably have a duplicate or invalid GUID, get us a new one.
                serializedGuid = null;
                _guid = Guid.Empty;
                CreateGuid();
            }
        }
    }

#if UNITY_EDITOR
    private bool IsEditingInPrefabMode()
    {
        if (EditorUtility.IsPersistent(this))
            return true;
        else
        {
            // If the GameObject is not persistent let's determine which stage we are in first because getting Prefab info depends on it
            var mainStage = StageUtility.GetMainStageHandle();
            var currentStage = StageUtility.GetStageHandle(gameObject);
            if (currentStage != mainStage)
            {
                var prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);
                if (prefabStage != null)
                    return true;
            }
        }

        return false;
    }

    private bool IsAssetOnDisk() => PrefabUtility.IsPartOfPrefabAsset(this) || IsEditingInPrefabMode();
#endif

    // We cannot allow a GUID to be saved into a prefab, and we need to convert to byte[]
    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        // A prefab asset cannot contain a GUID since it would then be duplicated when instanced.
        if (IsAssetOnDisk())
        {
            serializedGuid = null;
            _guid = Guid.Empty;
        }
        else
#endif
        {
            if (_guid != Guid.Empty)
                serializedGuid = _guid.ToByteArray();
        }
    }

    // On load, we can go head a restore our system guid for later use
    public void OnAfterDeserialize()
    {
        if (serializedGuid != null && serializedGuid.Length == 16)
            _guid = new Guid(serializedGuid);
    }

    void Awake()
    {
        CreateGuid();
    }

    void OnValidate()
    {
#if UNITY_EDITOR
        // similar to on Serialize, but gets called on Copying a Component or Applying a Prefab at a time that lets us detect what we are
        if (IsAssetOnDisk())
        {
            serializedGuid = null;
            _guid = Guid.Empty;
        }
        else
#endif
            CreateGuid();
    }

    // Never return an invalid GUID
    public Guid GetGuid()
    {
        if (_guid == Guid.Empty && serializedGuid != null && serializedGuid.Length == 16)
            _guid = new Guid(serializedGuid);

        return _guid;
    }

    // let the manager know we are gone, so other objects no longer find this
    public void OnDestroy()
    {
        GuidLookupTable.Remove(_guid);
    }
}