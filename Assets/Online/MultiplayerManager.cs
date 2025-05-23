using System.Runtime.CompilerServices;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using UnityEngine;

public class MultiplayerManager : MonoBehaviour
{
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private GameSettings defaultGameSettings;

    public static bool IsOnline = false;

    // Since relay always has one player as a host, this variable is true as long as the non-host client has joined
    public static bool NonHostClientJoined = false;
    public static bool GameStarted => (IsOnline && NonHostClientJoined) || !IsOnline;

    public static MultiplayerManager Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;

        if (GameSettings.Used == null)
        {
            Debug.Log($"No GameSettings set during spell selection (or spell selection was skipped). Setting GameSettings according to Multiplayer Manager.");
            GameSettings.Used = defaultGameSettings;
        }
    }
    private void Start()
    {
        if(IsOnline)
        {
            if (RelayManager.IsHost) StartRelayHost();
            else StartRelayClient();
        }
        else
        {
            for (byte i = 0; i < GameSettings.Used.MaxCharacters; i++)
            {
                SpawnCharacter(i);
            }
        }

        // Local Methods
        void SpawnCharacter(byte index)
        {
            GameObject character = Instantiate(characterPrefab);
            character.GetComponentInChildren<CharacterManager>().CharacterIndex = index;
        }
    }
    
    private void StartRelayHost()
    {
        Debug.Log($"Hosting with relay.");
        try
        {
            RelayServerData relayServerData = new(RelayManager.allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.LogWarning(e);
        }
    }
    private void StartRelayClient()
    {
        
        Debug.Log($"Joining as client with relay.");
        try
        {
            RelayServerData relayServerData = new(RelayManager.joinAllocation, "dtls");

            Debug.Log($"relay joined, starting client");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
            Debug.Log($"client started!");
        }
        catch (RelayServiceException e)
        {
            Debug.LogWarning(e);
        }
    }
}
