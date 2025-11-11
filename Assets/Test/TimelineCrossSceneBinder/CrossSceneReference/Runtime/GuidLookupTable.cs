using System;
using System.Collections.Generic;
using UnityEngine;

public class GuidLookupTable
{
    private struct GuidInfo
    {
        public GameObject gameObject;

        public event Action<GameObject> OnAdd;
        public event Action OnRemove;

        public GuidInfo(ObjectGuid comp)
        {
            gameObject = comp.gameObject;
            OnRemove = null;
            OnAdd = null;
        }

        public void HandleAddCallback() => OnAdd?.Invoke(gameObject);
        public void HandleRemoveCallback() => OnRemove?.Invoke();
    }

    // Singleton interface
    static GuidLookupTable Instance;

    // All the public API is static so you need not worry about creating an instance
    public static bool Add(ObjectGuid objectGuid)
    {
        if (Instance == null)
            Instance = new GuidLookupTable();

        return Instance.InternalAdd(objectGuid);
    }

    public static void Remove(Guid guid)
    {
        if (Instance == null)
            Instance = new GuidLookupTable();

        Instance.InternalRemove(guid);
    }

    public static GameObject ResolveGuid(Guid guid, Action<GameObject> onAddCallback = null, Action onRemoveCallback = null)
    {
        if (Instance == null)
            Instance = new GuidLookupTable();

        return Instance.ResolveGuidInternal(guid, onAddCallback, onRemoveCallback);
    }

    private Dictionary<Guid, GuidInfo> guidToObjectMap;

    private GuidLookupTable()
    {
        guidToObjectMap = new Dictionary<Guid, GuidInfo>();
    }

    private bool InternalAdd(ObjectGuid objectGuid)
    {
        Guid guid = objectGuid.GetGuid();
        if (!guidToObjectMap.TryGetValue(guid, out var existingInfo))
        {
            guidToObjectMap[guid] = new GuidInfo(objectGuid);
            return true;
        }

        if (existingInfo.gameObject != null && existingInfo.gameObject != objectGuid.gameObject)
        {
            LogGuidCollision(objectGuid, existingInfo.gameObject);
            return false;
        }

        // if we already tried to find this GUID, but haven't set the game object to anything specific, copy any OnAdd callbacks then call them
        existingInfo.gameObject = objectGuid.gameObject;
        existingInfo.HandleAddCallback();
        guidToObjectMap[guid] = existingInfo;
        return true;
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    private static void LogGuidCollision(ObjectGuid objectGuid, GameObject existingGO)
    {
        string existingName = existingGO ? existingGO.name : "NULL";
        string compName = objectGuid ? objectGuid.name : "NULL";
        string msg = Application.isPlaying
            ? $"Guid Collision Detected between {existingName} and {compName}.\n" +
              "Assigning new Guid. Consider tracking runtime instances using a direct reference or other method."
            : $"Guid Collision Detected while creating {compName}.\nAssigning new Guid.";

        if (Application.isPlaying)
            Debug.Assert(false, msg, objectGuid);
        else
            Debug.LogWarning(msg, objectGuid);
    }


    private void InternalRemove(Guid guid)
    {
        if (guidToObjectMap.TryGetValue(guid, out var info))
            info.HandleRemoveCallback();

        guidToObjectMap.Remove(guid);
    }

    private GameObject ResolveGuidInternal(Guid guid, Action<GameObject> onAddCallback, Action onRemoveCallback)
    {
        guidToObjectMap.TryGetValue(guid, out GuidInfo info);

        if (onAddCallback != null)
            info.OnAdd += onAddCallback;

        if (onRemoveCallback != null)
            info.OnRemove += onRemoveCallback;

        guidToObjectMap[guid] = info;
        return info.gameObject;
    }
}