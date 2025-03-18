using UnityEngine;
using System;

namespace SAS.StateMachineGraph.Utilities
{
    [CreateAssetMenu(menuName = "SAS/State Machine Character Controller/Awaitable Animator Parameters Config")]
    public class AwaitableAnimatorParametersConfig : ScriptableObject
    {
        [System.Serializable]
        public class ParametersKeyMap
        {
            public string key;
            public Parameter[] parameters;
            public string awaitableStateTag;
            public float startDelay;
        }

        [SerializeField] private ParametersKeyMap[] m_ParametersKeyMap;
        public bool TryGet(string key, out ParametersKeyMap parametersKeyMap)
        {
            parametersKeyMap = Array.Find(m_ParametersKeyMap, ele => ele.key.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (parametersKeyMap == null)
            {
                Debug.LogWarning($"No parameters has been found wrt the key:  {key} under the Parameter config SO : {this.name}");
                return false;
            }
            return true;
        }
    }
}
