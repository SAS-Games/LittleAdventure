using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using ZLinq;

public interface IReady
{
    bool IsReady { get; }
}

public class ReadyDependencyGroup : MonoBehaviour, IReady
{
    [SerializeField]
    private List<SerializableInterface<IReady>> m_Dependencies = new();

    private List<IReady> _dependencies = new();
    private TaskCompletionSource<bool> _readyTCS = new();

    public bool IsReady => _readyTCS.Task.IsCompleted;

    private void Awake()
    {
        foreach (var dependency in m_Dependencies)
        {
            _dependencies.Add(dependency.Value);
        }

        StartCoroutine(CheckDependencies());
    }

    private IEnumerator CheckDependencies()
    {
        while (true)
        {
            if (_dependencies.AsValueEnumerable().All(dep => dep is { IsReady: true }))
            {
                if (!_readyTCS.Task.IsCompleted)
                    _readyTCS.SetResult(true);
                yield break;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    public Task WaitUntilReadyAsync() => _readyTCS.Task;
    
}