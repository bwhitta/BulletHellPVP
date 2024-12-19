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

    private void Awake()
    {
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
            SpawnCharacter(0);
            SpawnCharacter(1);
        }

        void SpawnCharacter(byte index)
        {
            GameObject character = Instantiate(characterPrefab);
            character.GetComponent<CharacterManager>().CharacterIndex = index;
        }
    }

    private void StartRelayHost()
    {
        Debug.Log($"Hosting with relay.");
        try
        {
            RelayServerData relayServerData = new(RelayManager.allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData); // Replacing SetHostRelayData

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

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData); // Replacing SetClientRelayData

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogWarning(e);
        }
    }
}
