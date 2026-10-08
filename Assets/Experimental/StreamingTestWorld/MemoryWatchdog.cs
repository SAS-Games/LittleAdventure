using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;

public class MemoryWatchdog : MonoBehaviour
{
    public static MemoryWatchdog Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("In MB. When memory usage goes above this, cleanup will run.")]
    [SerializeField] private int m_MemoryThresholdMB = 1024;

    [Tooltip("How often to check memory usage (seconds)")]
    [SerializeField] private float m_CheckInterval = 5f;

    [Tooltip("Minimum delay between two cleanup operations (seconds)")]
    [SerializeField] private float m_CleanupCooldown = 30f;

    [Tooltip("Run a full managed garbage collection after unloading unused assets. This can cause a frame hitch.")]
    [SerializeField] private bool m_ForceGarbageCollection;

    [Tooltip("Optional runtime memory readout.")]
    [SerializeField] private TMP_Text m_text;

    private bool _isCleaning;
    private float _lastCleanupTime;
    private WaitForSecondsRealtime _waitForSeconds;

    public long CurrentAllocatedBytes { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _lastCleanupTime = float.NegativeInfinity;
            UpdateMemoryUsageText();
            _waitForSeconds = new WaitForSecondsRealtime(m_CheckInterval);
            StartCoroutine(CheckMemoryRoutine());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private IEnumerator CheckMemoryRoutine()
    {
        while (true)
        {
            yield return _waitForSeconds;
            UpdateMemoryUsageText();
            TryCleanup();
        }
    }
    
    private void UpdateMemoryUsageText()
    {
        CurrentAllocatedBytes = Profiler.GetTotalAllocatedMemoryLong();
        if (m_text != null)
        {
            float usedGB = CurrentAllocatedBytes / (1024f * 1024f * 1024f);
            m_text.text = $"Memory Used {usedGB:F2} GB";
        }
    }
    /// <summary>
    /// Request memory cleanup.
    /// Set force = true to ignore thresholds and cooldown.
    /// </summary>
    public static void RequestCleanup(bool force = false)
    {
        if (Instance != null)
            Instance.TryCleanup(force);
    }

    private void TryCleanup(bool force = false)
    {
        if (_isCleaning) return;

        if (!force)
        {
            long thresholdBytes = (long)m_MemoryThresholdMB * 1024 * 1024;
            long usedBytes = CurrentAllocatedBytes = Profiler.GetTotalAllocatedMemoryLong();

            if (usedBytes < thresholdBytes) 
                return;
            if (Time.realtimeSinceStartup - _lastCleanupTime < m_CleanupCooldown) 
                return;

            Debug.LogWarning($"[MemoryWatchdog] Memory {usedBytes / (1024 * 1024)}MB exceeded threshold {m_MemoryThresholdMB}MB. Cleaning up...");
        }
        else
            Debug.LogWarning("[MemoryWatchdog] Forced cleanup requested.");

        StartCoroutine(RunCleanup());
    }

    private IEnumerator RunCleanup()
    {
        _isCleaning = true;
        yield return Resources.UnloadUnusedAssets();
        if (m_ForceGarbageCollection)
            GC.Collect();
        _lastCleanupTime = Time.realtimeSinceStartup;
        _isCleaning = false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        m_MemoryThresholdMB = Mathf.Max(1, m_MemoryThresholdMB);
        m_CheckInterval = Mathf.Max(0.1f, m_CheckInterval);
        m_CleanupCooldown = Mathf.Max(0f, m_CleanupCooldown);

        if (Application.isPlaying && Instance == this)
            _waitForSeconds = new WaitForSecondsRealtime(m_CheckInterval);
    }
#endif
}
