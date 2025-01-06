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
    private float heartbeatTimer;
    private string playerName;

    [Header("Lobby Info")]
    [SerializeField] private byte maxPlayers;
    [SerializeField] private float lobbbyHeartbeatFrequency;
    [SerializeField] private string spellSelectionScene;
    [SerializeField] private string gameplayScene;

    private const string KEY_START_GAME = "Start";
    private const string KEY_SELECTING_SPELLS = "SelectingSpells";
    private const string KEY_PLAYER_NAME = "PlayerName";

    private Lobby hostLobby; // I'll be honest I have no clue why I have two variables that both just say what lobby you're in, but I think that's what the tutorial had told me to do and I have no reason to change it yet.
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
    }

    // Sign in to unity services
    private async void SignIn()
    {
        // Connect to unity services
        await UnityServices.InitializeAsync();

        // Sign in anonomously. To make actions occur upon sign in: AuthenticationService.Instance.SignedIn += signedInActions;
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        DontDestroyOnLoad(gameObject);

        // Set up UI
        ToggleLobbySelectionButtons(true);
        loadingText.SetActive(false);

        playerName = $"Player_{AuthenticationService.Instance.PlayerId.ToString()[..5]}";
        Debug.Log($"Player Name: {playerName}");
    }

    // Sends a "heartbeat" every while to let unity services know that the lobby is still active
    private async void HeartbeatPing()
    {
        if (hostLobby == null) return;

        heartbeatTimer += Time.deltaTime;
        if (heartbeatTimer >= lobbbyHeartbeatFrequency)
        {
            heartbeatTimer = 0;
            Debug.Log("heartbeat ping");

            await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
        }
    }

    // Lobby events
    private async void LobbyEventSubscription(Lobby lobby)
    {
        // Subscribe to lobby events
        LobbyEventCallbacks callbacks;
        callbacks = new LobbyEventCallbacks();
        callbacks.LobbyChanged += OnLobbyChanged;
        // callbacks.KickedFromLobby += OnKickedFromLobby; // potentially implement later
        // callbacks.LobbyEventConnectionStateChanged += OnLobbyEventConnectionStateChanged; // probably will want to use this some more later
        try
        {
            Debug.Log("Subscribing to lobby events");
            //var lobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks); // is var right here? should I declare lobbyEvents elsewhere?
            await Lobbies.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);
        }
        catch (LobbyServiceException ex)
        {
            switch (ex.Reason)
            {
                case LobbyExceptionReason.AlreadySubscribedToLobby: Debug.LogWarning($"Already subscribed to lobby[{lobby.Id}]. We did not need to try and subscribe again. Exception Message: {ex.Message}"); break;
                case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy: Debug.LogError($"Subscription to lobby events was lost while it was busy trying to subscribe. Exception Message: {ex.Message}"); throw;
                case LobbyExceptionReason.LobbyEventServiceConnectionError: Debug.LogError($"Failed to connect to lobby events. Exception Message: {ex.Message}"); throw;
                default: throw;
            }
        }
    }
    private void OnLobbyChanged(ILobbyChanges changes)
    {
        changes.ApplyToLobby(joinedLobby);

        if (changes.PlayerData.Changed)
        {
            Debug.Log($"Lobby changed: Player data changed");
            UpdateLobbyVisuals(joinedLobby);
        }
        if (changes.PlayerLeft.Changed)
        {
            Debug.Log($"Player left");
            UpdateLobbyVisuals(joinedLobby);
        }
        if (changes.PlayerJoined.Changed)
        {
            Debug.Log("Player joined");
            UpdateLobbyVisuals(joinedLobby);
            // lobby visuals or something should be changed here to show the new player as soon as they join.
        }
        if (changes.Data.Changed)
        {
            // Check if spell selection started
            if (joinedLobby.Data[KEY_SELECTING_SPELLS].Value == "true" && SceneManager.GetActiveScene().name != spellSelectionScene && !IsLobbyHost)
            {
                Debug.Log($"Starting spell selection");
                ClientLoadSpellSelection();
            }
            // Check if the game was started
            if (joinedLobby.Data[KEY_START_GAME].Value != "0")
            {
                Debug.Log($"Starting game");
                if (!IsLobbyHost)
                {
                    ClientStartGame();
                }

                joinedLobby = null;
            }
        }
        if (changes.Data.Added || changes.Data.Removed) Debug.LogWarning($"Lobby changed: Data added or removed");
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

            // Subscribe to lobby events
            LobbyEventSubscription(createdLobby);

            // Show play button
            lobbyPlayButton.SetActive(true);

            Debug.Log($"Created lobby {createdLobby} (Lobby Id: {createdLobby.Id}, Lobby code: {createdLobby.LobbyCode}).");
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

            // Subscribe to lobby events
            LobbyEventSubscription(createdLobby);

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
            Player = FindPlayerData(),
            Data = new Dictionary<string, DataObject>
                {
                    { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "0") }, 
                    { KEY_SELECTING_SPELLS, new DataObject(DataObject.VisibilityOptions.Member, "false")}
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
                Player = FindPlayerData()
            };

            // Join lobby
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            joinedLobby = lobby;

            // Subscribe to lobby events
            LobbyEventSubscription(lobby);

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
                Player = FindPlayerData()
            };

            // Join the lobby
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCodeField.text, options);
            joinedLobby = lobby;

            // Subscribe to lobby events
            LobbyEventSubscription(lobby);

            Debug.Log($"Joined lobby ({lobby.Name}) with code {lobby.LobbyCode}.");
            UpdateLobbyVisuals(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning(e);
        }
    }

    // Scene loading
    public async void HostLoadSpellSelection()
    {
        Debug.Log($"Loading spell selection");
        try
        {
            // Update the lobby options to say that spell selection has begun
            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {  KEY_SELECTING_SPELLS, new DataObject(DataObject.VisibilityOptions.Member, "true") }
                }
            });
            joinedLobby = lobby;

            // Load spell selection
            SceneManager.LoadScene(spellSelectionScene);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning(e);
        }
    }
    public void ClientLoadSpellSelection()
    {
        Debug.Log($"Client loading spell selection.");
        SceneManager.LoadScene(spellSelectionScene);
    }
    public async void HostStartGame()
    {
        Debug.Log($"Starting game");
        try
        {
            string relayCode = await RelayManager.Instance.CreateRelay();
            if (relayCode == null)
            {
                Debug.LogWarning($"Relay code null, cancelling start of game");
                return;
            }

            // Update the lobby options to say that the game is starting
            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_START_GAME , new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                }
            });

            joinedLobby = lobby;
            SceneManager.LoadScene(gameplayScene);

            Destroy(gameObject);
            Debug.Log($"Destroying lobby manager");
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

        Debug.Log($"joined relay, loading scene {gameplayScene}");
        SceneManager.LoadScene(gameplayScene);

        Debug.Log($"Destroying lobby manager");
        Destroy(gameObject);
    }

    // Finds your personal player data
    private Player FindPlayerData()
    {
        Player player = new()
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
            }
        };
        return player;
    }

    // Update what the lobby looks like (e.g. who is in it and their names)
    private void UpdateLobbyVisuals(Lobby lobby)
    {
        //Debug.Log($"Updating lobby visuals!");

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
            Debug.Log($"lobby player count discrepancy, {lobby.Players.Count} is not equal to {lobbyPlayerObjects.Length}");
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
            lobbyPlayerObjects = new GameObject[lobby.Players.Count];
            
            for (int i = 0; i < lobby.Players.Count; i++)
            {

                // Create lobby player object
                Debug.Log($"creating lobby player {i + 1}");
                GameObject lobbyPlayer = Instantiate(lobbyPlayerPrefab, canvasObject.transform);

                // Update visuals and position
                lobbyPlayer.GetComponentInChildren<TextMeshProUGUI>().text = lobby.Players[i].Data["PlayerName"].Value;
                lobbyPlayer.GetComponent<RectTransform>().anchoredPosition = new(0, lobbyTopPlayerY + (lobbyPlayerOffsetY * (i)));

                lobbyPlayerObjects[i] = lobbyPlayer;
            }
        }
    }
}