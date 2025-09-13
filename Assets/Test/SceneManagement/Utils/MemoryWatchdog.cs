using System.Collections;
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

    private bool _isCleaning;
    private float _lastCleanupTime;
    private WaitForSeconds _waitForSeconds;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _waitForSeconds = new WaitForSeconds(m_CheckInterval);

            StartCoroutine(CheckMemoryRoutine());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator CheckMemoryRoutine()
    {
        while (true)
        {
            yield return _waitForSeconds;
            TryCleanup();
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
            long usedBytes = Profiler.GetTotalAllocatedMemoryLong();

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
        System.GC.Collect();
        _lastCleanupTime = Time.realtimeSinceStartup;
        _isCleaning = false;
    }
}
