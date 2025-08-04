using SAS.Utilities.TagSystem;
using UniRx;
using UnityEngine;
public interface IAnimatorProcessor
{
    string Tag { get; }
    void Process(string value);
    IReadOnlyReactiveProperty<string> AnimatorState { get; }
}

namespace SAS.DialogueSystem
{
    public class ProxyAnimatorProcessor : MonoBehaviour, IAnimatorProcessor
    {
        [SerializeField] private string m_Tag;

        private readonly ReactiveProperty<string> _animatorState = new ReactiveProperty<string>();
        public IReadOnlyReactiveProperty<string> AnimatorState => _animatorState;
        string IAnimatorProcessor.Tag => m_Tag;


        void IAnimatorProcessor.Process(string tagValue)
        {
            _animatorState.Value = tagValue;
        }
    }
}