using System.Collections.Generic;
using System.Linq;
using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PlayerConfigurationUI : UIScreenView
{
    [SerializeField] private GameObject[] m_Players;
    [SerializeField] private PlayerSetupMenu m_PlayerConfigScreen;
    [SerializeField] private RectTransform m_Content;
    [SerializeField] private SceneGroupLoader m_SceneGroupLoader;
    [SerializeField] private PlayerNamesConfig m_AvailableNamesConfig;
    [SerializeField] private ColorConfig m_ColorConfig;
    private IReadOnlyList<string> _nameOptions;
    private IReadOnlyList<string> _colorOptions;
    [Inject] private IPlayerSetupModel _playerSetupModel;

    private readonly Dictionary<int, string> _chosenNames = new();
    private readonly Dictionary<int, string> _chosenColors = new();
    private readonly Dictionary<int, PlayerSetupMenu> _playerMenus = new();
    private readonly HashSet<int> _readyPlayerIndices = new();

    protected override void Awake()
    {
        base.Awake();
        this.InjectFieldBindings();
        PlayerSetupController.Instance.Clear();
        _readyPlayerIndices.Clear();
        _chosenNames.Clear();
        _chosenColors.Clear();
        _playerMenus.Clear();
        _nameOptions = m_AvailableNamesConfig.AvailableNames;
        _colorOptions = m_ColorConfig.ColorNames;
    }

    public void OnPlayerJoin(PlayerInput playerInput)
    {
        PlayerSetupController.Instance.HandlePlayerJoin(playerInput);
        var menu = Instantiate(m_PlayerConfigScreen, m_Content.transform);
        playerInput.uiInputModule = menu.GetComponentInChildren<InputSystemUIInputModule>();

        int playerIndex = playerInput.playerIndex;
        _playerMenus[playerIndex] = menu;

        m_Players[playerIndex].SetActive(true);
        _playerSetupModel.Players[playerIndex].Character = m_Players[playerIndex];

        string defaultName = GetAvailableNames(playerIndex).FirstOrDefault();
        string defaultColor = GetAvailableColors(playerIndex).FirstOrDefault();
        OnNameChosen(playerIndex, defaultName);
        OnColorChosen(playerIndex, defaultColor);

        menu.SetPlayerIndex(playerIndex);
        menu.SetNameOptions(GetAvailableNames(playerIndex), defaultName);
        menu.SetColorOptions(GetAvailableColors(playerIndex), defaultColor);

        menu.OnNameSelected += OnNameChosen;
        menu.OnColorSelected += OnColorChosen;
        menu.OnPlayerMarkedReady += OnPlayerReady;
    }

    private void OnNameChosen(int playerIndex, string selectedName)
    {
        _chosenNames[playerIndex] = selectedName;
        _playerSetupModel.Players[playerIndex].DisplayName = selectedName;
        UpdateAllNameDropdowns();
    }

    private void OnColorChosen(int playerIndex, string color)
    {
        _chosenColors[playerIndex] = color;
        Color selectedColor = m_ColorConfig.GetColor(color);
        _playerSetupModel.Players[playerIndex].Color = selectedColor;
        var character = _playerSetupModel.Players[playerIndex].Character;
        SkinnedMeshRenderer skinnedRenderer = character.GetComponentInChildren<SkinnedMeshRenderer>();
        Material material = skinnedRenderer.material;
        material.SetColor("_BaseColor", selectedColor);
        UpdateAllColorDropdowns();
    }

    private void UpdateAllNameDropdowns()
    {
        foreach (var kvp in _playerMenus)
        {
            int playerIndex = kvp.Key;
            var menu = kvp.Value;
            menu.SetNameOptions(GetAvailableNames(playerIndex), GetCurrentSelectedName(playerIndex));
        }
    }

    private void UpdateAllColorDropdowns()
    {
        foreach (var kvp in _playerMenus)
        {
            int playerIndex = kvp.Key;
            var menu = kvp.Value;
            menu.SetColorOptions(GetAvailableColors(playerIndex), GetCurrentSelectedColor(playerIndex));
        }
    }

    private string GetCurrentSelectedName(int playerIndex)
    {
        return _chosenNames.GetValueOrDefault(playerIndex);
    }

    private string GetCurrentSelectedColor(int playerIndex)
    {
        return _chosenColors.GetValueOrDefault(playerIndex);
    }

    private List<string> GetAvailableNames(int requestingPlayer)
        => GetAvailableOptions(_nameOptions, _chosenNames, requestingPlayer);

    private List<string> GetAvailableColors(int requestingPlayer)
        => GetAvailableOptions(_colorOptions, _chosenColors, requestingPlayer);


    private List<string> GetAvailableOptions(IReadOnlyList<string> allOptions, Dictionary<int, string> chosenMap,
        int requestingPlayer)
    {
        var used = new HashSet<string>(chosenMap.Values);

        if (chosenMap.TryGetValue(requestingPlayer, out var current))
            used.Remove(current);

        var available = new List<string>();
        foreach (var option in allOptions)
        {
            if (!used.Contains(option))
                available.Add(option);
        }

        return available;
    }

    private void OnPlayerReady(int playerIndex)
    {
        _readyPlayerIndices.Add(playerIndex);
        if (_readyPlayerIndices.Count == PlayerSetupController.Instance.MaxPlayers)
            m_SceneGroupLoader.Load();
    }
}