using System;
using SAS.TweenManagement;
using UnityEngine;
using UnityEngine.UI;

namespace SAS.SceneManagement
{
    public enum FadeType
    {
        Alpha,
        Shutter,
        RadialWipe,
        GradientTexture
    }

    [Serializable]
    public class SpriteArrayWrapper
    {
        [ConditionalField("FadeType", FadeType.GradientTexture)]
        [SerializeField] private Sprite[] m_Sprites;
        public int Length => m_Sprites.Length;
        public Texture GetRandomTexture()
        {
            return m_Sprites[UnityEngine.Random.Range(0, m_Sprites.Length)].texture;
        }
    }

    public class SceneFader : MonoBehaviour, ILoadingScreen
    {
        [SerializeField] private FadeType m_FadeType;
        [ConditionalField(nameof(m_FadeType), FadeType.GradientTexture)]
        [SerializeField] private SpriteArrayWrapper m_FadePattern;
        [SerializeField] private Image m_Image;
        [SerializeField] private float m_FadeInDuration = 1f;
        [SerializeField] private float m_FadeOutDuration = 1f;
        private int _fadeAmount = Shader.PropertyToID("_Amount");
        private int _useShutters = Shader.PropertyToID("_UseShutters");
        private int _useRadialWipe = Shader.PropertyToID("_UseRadialWipe");
        private int _useGradientTexture = Shader.PropertyToID("_UseGradientTexture");
        private int _useAlpha = Shader.PropertyToID("_UseAlpha");
        private int _mainTex = Shader.PropertyToID("_MainTex");

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

            FaderSetup(m_FadeType);

            _fadeTween?.Stop(true);
            _fadeTween = active
                ? Tween.CreateTween(0f, 1f, FadeTween, ref _fadeInTweenConfig)
                : Tween.CreateTween(1f, 0f, FadeTween, ref _fadeOutTweenConfig);
            _fadeTween.Run();
        }

        private void FaderSetup(FadeType fadeType)
        {
            ResetAllFadeType();

            switch (fadeType)
            {
                case FadeType.Shutter:
                    SwitchEffect(_useShutters);
                    break;
                case FadeType.Alpha:
                    SwitchEffect(_useAlpha);
                    break;
                case FadeType.RadialWipe:
                    SwitchEffect(_useRadialWipe);
                    break;
                case FadeType.GradientTexture:
                    _material.SetTexture(_mainTex, m_FadePattern.GetRandomTexture());
                    SwitchEffect(_useGradientTexture);
                    break;
            }

        }

        private void SwitchEffect(int effectToTurnOn)
        {
            _material.SetFloat(effectToTurnOn, 1);
            _lastEffect = effectToTurnOn;
        }

        private void ResetAllFadeType()
        {
            _material.SetFloat(_useAlpha, 0);
            _material.SetFloat(_useShutters, 0);
            _material.SetFloat(_useRadialWipe, 0);
            _material.SetFloat(_useGradientTexture, 0);
        }

        public Action OnFadeInComplete { get; set; }
        public Action OnFadeOutComplete { get; set; }

        private void FadeTween(float value)
        {
            _material.SetFloat(_fadeAmount, value);
        }
    }
}