using System;
using SAS.StateMachineCharacterController;
using UniRx;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

struct GamePauseEvent : IEvent
{
    public bool state;
}

struct GameOverEvent : IEvent
{
}

struct LevelCompleteEvent : IEvent
{
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject m_PlayerPrefab;
    private bool _gamePaused = false;

    private void Start()
    {
        var player = m_PlayerPrefab; //Instantiate(m_PlayerPrefab);
        
        CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
        (brain.ActiveVirtualCamera as CinemachineCamera).Target.TrackingTarget =
            player.GetComponent<ICameraLookAt>().Target;
        player.GetComponent<IHealthPresenter>().HealthModel.OnDeath.Subscribe(_ => { GameOver(); }).AddTo(this);
    }

    public void PauseGame()
    {
        _gamePaused = !_gamePaused;
        EventBus<GamePauseEvent>.Raise(new GamePauseEvent { state = _gamePaused });
    }

    private void GameOver()
    {
        EventBus<GameOverEvent>.Raise(new GameOverEvent { });
    }

    public static void LevelComplete()
    {
        EventBus<GameOverEvent>.Raise(new GameOverEvent { });
    }
}