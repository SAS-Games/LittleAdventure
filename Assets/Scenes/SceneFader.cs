using System;
using SAS.TweenManagement;
using UnityEngine;
using UnityEngine.UI;

namespace SAS.SceneManagement
{
    public class SceneFader : MonoBehaviour, ILoadingScreen
    {
        [SerializeField] private Image m_Image;
        [SerializeField] private float m_FadeInDuration = 1f;
        [SerializeField] private float m_FadeOutDuration = 1f;
        private int _fadeAmount = Shader.PropertyToID("_Amount");
        private int _useShutters = Shader.PropertyToID("_UseShutters");

        private int? _lastEffect;
        private ITween _fadeTween;
        private TweenConfig _fadeInTweenConfig;
        private TweenConfig _fadeOutTweenConfig;
        private Material _material;

        void Awake()
        {
            _fadeInTweenConfig = new TweenConfig().Duration(m_FadeInDuration)
                .TweenCompleteCallback(_ => OnFadeInComplete?.Invoke());
            _fadeOutTweenConfig = new TweenConfig().Duration(m_FadeOutDuration)
                .TweenCompleteCallback(_ =>
                    {
                        OnFadeOutComplete?.Invoke();
                        gameObject.SetActive(false);
                    }
                );

            var material = m_Image.material;
            m_Image.material = new Material(material);
            _material = m_Image.material;
        }

        public void SetActive(bool active)
        {
            if (active)
                gameObject.SetActive(true);
            _material.SetFloat(_useShutters, 1);
            _fadeTween?.Stop(true);
            _fadeTween = active
                ? Tween.CreateTween(0f, 1f, FadeTween, ref _fadeInTweenConfig)
                : Tween.CreateTween(1f, 0f, FadeTween, ref _fadeOutTweenConfig);
            _fadeTween.Run();
        }

        public Action OnFadeInComplete { get; set; }
        public Action OnFadeOutComplete { get; set; }

        private void FadeTween(float value)
        {
            _material.SetFloat(_fadeAmount, value);
        }
    }
}