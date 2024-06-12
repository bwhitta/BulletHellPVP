using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    private float heartbeatTimer, lobbyUpdatePollTimer;
    private string playerName;

    [Header("Lobby Info")]
    [SerializeField] private byte maxPlayers;
    [SerializeField] private float lobbyOpenPingFrequency;
    [SerializeField] private float lobbyUpdatePollFrequency;
    [SerializeField] private string gameplayScene;
    public const string KEY_START_GAME = "Start";
    private Lobby hostLobby;
    private Lobby joinedLobby;
    public bool IsLobbyHost => joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;

    [Space][Header("Text and Display")]
    [SerializeField] private TMP_InputField lobbyCodeField;
    [SerializeField] private GameObject[] lobbySelectionButtons;
    [SerializeField] private GameObject loadingText;
    [SerializeField] private TextMeshProUGUI lobbyInfo;

    [Space][Header("Lobby Graphics")]
    [SerializeField] private GameObject lobbyPlayerPrefab;
    [SerializeField] private GameObject canvasObject;
    [SerializeField] private GameObject lobbyPlayButton;
    [SerializeField] private float lobbyTopPlayerY;
    [SerializeField] private float lobbyPlayerOffsetY;
    private GameObject[] lobbyPlayerObjects;


    private void Start()
    {
        SignIn();
    }
    private void Update()
    {
        HeartbeatPing();
        HandleLobbyUpdatePoll();
    }

    // Sends a "heartbeat" every while to let unity services know that the lobby is still active
    private async void HeartbeatPing()
    {
        if (hostLobby == null) return;

        heartbeatTimer += Time.deltaTime;
        if (heartbeatTimer >= lobbyOpenPingFrequency)
        {
            heartbeatTimer = 0;
            Debug.Log("heartbeat ping");
            await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
        }
    }
    
    // Checks every few seconds to see if anything about the lobby has changed.
    private async void HandleLobbyUpdatePoll()
    {
        if (joinedLobby == null) return;

        lobbyUpdatePollTimer += Time.deltaTime;
        if (lobbyUpdatePollTimer >= lobbyUpdatePollFrequency)
        {
            lobbyUpdatePollTimer = 0;
            Debug.Log("lobby update poll");

            // Updates the joinedLobby's info.
            joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);

            UpdateLobbyVisuals(joinedLobby);

            Debug.Log($"Joined lobby: {joinedLobby}");
            Debug.Log($"Joined lobby data: {joinedLobby}");
            if (joinedLobby.Data[KEY_START_GAME].Value != "0")
            {
                Debug.Log($"If this is a client, starting game!");
                if (!IsLobbyHost)
                {
                    ClientStartGame();
                }

                joinedLobby = null;
            }
        }
        else
        {
            lobbyUpdatePollTimer += Time.deltaTime;
        }
    }


    // Sign in to unity services
    private async void SignIn()
    {
        // Connect to unity services
        await UnityServices.InitializeAsync();
        
        // Sign in anonomously. To make actions occur upon sign in: AuthenticationService.Instance.SignedIn += signedInActions;
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        // Set up UI
        ToggleLobbySelectionButtons(true);
        loadingText.SetActive(false);
        
        playerName = $"Player_{AuthenticationService.Instance.PlayerId.ToString()[..5]}";
        Debug.Log($"Player Name: {playerName}");
    }

    // Toggle the Host and Join buttons
    private void ToggleLobbySelectionButtons(bool enabled)
    {
        foreach (GameObject button in lobbySelectionButtons)
        {
            button.SetActive(enabled);
        }
    }

    // Lobby Creation
    public async void CreatePublicLobby()
    {
        try
        {
            Debug.Log($"Creating public lobby");
            ToggleLobbySelectionButtons(false);
            string lobbyName = $"{AuthenticationService.Instance.PlayerId}'s Public Lobby";

            // Lobby options
            CreateLobbyOptions lobbyOptions = LobbyOptions(false);

            // Create the lobby
            Lobby createdLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, lobbyOptions);
            hostLobby = createdLobby;
            joinedLobby = createdLobby;

            // Show play button
            lobbyPlayButton.SetActive(true);

            Debug.Log($"Created lobby {createdLobby} (Lobby ID: {createdLobby.Id}, Lobby code: {createdLobby.LobbyCode}).");
            UpdateLobbyVisuals(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning(e);
        }
    }
    public async void CreatePrivateLobby()
    {
        try
        {
            Debug.Log($"Creating private lobby");
            ToggleLobbySelectionButtons(false);
            string lobbyName = $"{AuthenticationService.Instance.PlayerId}'s Private Lobby";

            // Lobby options
            CreateLobbyOptions lobbyOptions = LobbyOptions(true);

            // Create the lobby
            Lobby createdLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, lobbyOptions);
            hostLobby = createdLobby;
            joinedLobby = createdLobby;

            // Show play button
            lobbyPlayButton.SetActive(true);
            
            Debug.Log($"Created lobby {createdLobby} (Lobby ID: {createdLobby.Id}, Lobby code: {createdLobby.LobbyCode}).");
            UpdateLobbyVisuals(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning(e);
        }
    }
    CreateLobbyOptions LobbyOptions(bool privateLobby)
    {
        return new()
        {
            IsPrivate = privateLobby,
            Player = GetPlayer(),
            Data = new Dictionary<string, DataObject>
                {
                    { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "0") }
                }
        };
    }

    // Lobby Joining
    public async void JoinPublicLobby()
    {
        try
        {
            ToggleLobbySelectionButtons(false);

            // Filters joinable lobbies to lobbies that have at least one available slot.
            QuickJoinLobbyOptions options = new()
            {
                Filter = new List<QueryFilter>()
                {
                    new(field: QueryFilter.FieldOptions.AvailableSlots,
                                    op: QueryFilter.OpOptions.GE,
                                    value: "1")
                },
                Player = GetPlayer()
            };

            // Join lobby
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            joinedLobby = lobby;

            Debug.Log($"Joined lobby {lobby.Name}");
            UpdateLobbyVisuals(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning(e);
        }
    }
    public async void JoinPrivateLobby()
    {
        try
        {
            ToggleLobbySelectionButtons(false);
            
            // Lobby joining options
            JoinLobbyByCodeOptions options = new()
            {
                Player = GetPlayer()
            };

            // Join the lobby
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCodeField.text, options);
            joinedLobby = lobby;

            Debug.Log($"Joined lobby ({lobby.Name}) with code {lobby.LobbyCode}.");
            UpdateLobbyVisuals(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning(e);
        }
    }

    // Start game
    public async void StartGame()
    {
        Debug.Log($"Starting game");
        try
        {
            string relayCode = await RelayManager.Instance.CreateRelay();
            if (relayCode == null)
            {
                Debug.Log($"Relay code null, cancelling start of game");
                return;
            }

            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_START_GAME , new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                }
            });
            joinedLobby = lobby;
            SceneManager.LoadScene(gameplayScene);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning(e);
        }
    }
    public async void ClientStartGame()
    {
        Debug.Log($"Client starting game.");
        await RelayManager.Instance.JoinRelay(joinedLobby.Data[KEY_START_GAME].Value);
        Debug.Log($"joined relay, loading scene {gameplayScene}, [{Enum.GetName(typeof(RelayManager.InstanceModes), RelayManager.LocalInstanceMode)}]");
        SceneManager.LoadScene(gameplayScene);
    }

    // Finds your personal player data
    private Player GetPlayer()
    {
        Player player = new()
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
                    }
        };
        return player;
    }

    // Update what the lobby looks like (e.g. who is in it and their names)
    private void UpdateLobbyVisuals(Lobby lobby)
    {
        // Skip if no lobby is joined
        if (joinedLobby == null) return;
        // Set info text
        lobbyInfo.text = $"Lobby Code: {lobby.LobbyCode}";

        if (lobbyPlayerObjects == null)
        {
            SetLobbyPlayerObjects();
        }
        else if (lobby.Players.Count != lobbyPlayerObjects.Length)
        {
            // Remove previous objects
            if (lobbyPlayerObjects != null)
            {
                foreach (GameObject lobbyPlayer in lobbyPlayerObjects)
                {
                    Destroy(lobbyPlayer);
                }
            }
            SetLobbyPlayerObjects();
        }
        
        void SetLobbyPlayerObjects()
        {
            // Set up the array to store the objects
            lobbyPlayerObjects = new GameObject[lobby.Players.Count];
            
            
            Debug.Log($"Creating {lobby.Players.Count} lobby player objects.");
            for (int i = 0; i < lobby.Players.Count; i++)
            {

                // Create lobby player object
                Debug.Log($"creating player {i + 1}");
                GameObject lobbyPlayer = Instantiate(lobbyPlayerPrefab, canvasObject.transform);

                // Update visuals and position
                lobbyPlayer.GetComponentInChildren<TextMeshProUGUI>().text = lobby.Players[i].Data["PlayerName"].Value;
                lobbyPlayer.GetComponent<RectTransform>().anchoredPosition = new(0, lobbyTopPlayerY + (lobbyPlayerOffsetY * (i)));

                lobbyPlayerObjects[i] = lobbyPlayer;
            }
        }
    }

}