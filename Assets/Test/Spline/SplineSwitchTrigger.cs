using System.Collections.Generic;
using UnityEngine;
using Debug = SAS.Debug;

[RequireComponent(typeof(SplineSwitcher))]
public class SplineSwitchTrigger : MonoBehaviour
{
    [SerializeField] private List<int> m_ActivePathData;
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        var splineFollow = other.GetComponentInParent<SplineFollow>();
        if (splineFollow != null)
        {
            SplineSwitcher switcher = this.GetComponent<SplineSwitcher>();
            switcher.SwitchToPath(splineFollow, m_ActivePathData);
        }
    }
}
