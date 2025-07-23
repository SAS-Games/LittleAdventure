using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class TestThread : MonoBehaviour
{
    [SerializeField] private GameObject m_SceneLoader;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        yield return new WaitForSeconds(10);
      m_SceneLoader.SetActive(true);
    }
}
