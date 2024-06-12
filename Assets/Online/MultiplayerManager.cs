using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using UnityEngine;

public class MultiplayerManager : MonoBehaviour
{
    public enum MultiplayerTypes { Local, Online }
    public static MultiplayerTypes multiplayerType;
    [SerializeField] private GameObject characterPrefab; // what does this do? where is it used?
    [SerializeField] private GameSettings defaultGameSettings;

    public static bool IsOnline 
    {
        get
        {
            return multiplayerType != MultiplayerTypes.Local;
        }
    }

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
        switch (multiplayerType)
        {
            case MultiplayerTypes.Online:
                StartOnline();
                break;
            case MultiplayerTypes.Local:
                Instantiate(characterPrefab);
                Instantiate(characterPrefab);
                break;
            default:
                Debug.LogWarning($"No multiplayer type detected, setting to default (local multiplayer).");
                multiplayerType = MultiplayerTypes.Local;
                Instantiate(characterPrefab);
                Instantiate(characterPrefab);
                break;
        }

        void StartOnline(){
            // PROBABLY PUT THIS ALL IN A SINGLE LOCAL METHOD CALLED FROM LINE 34!!!!! 
            Debug.Log($"Multiplayer manager start [{Enum.GetName(typeof(RelayManager.InstanceModes), RelayManager.LocalInstanceMode)}]");
            switch (RelayManager.LocalInstanceMode)
            {
                case RelayManager.InstanceModes.Host:
                    StartRelayHost();
                    break;
                case RelayManager.InstanceModes.Client:
                    StartRelayClient();
                    break;
                default:
                    Debug.LogError($"This InstanceMode is not implemented.");
                    break;
            }
        }
    }

    // Host relay
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

    // Join relay
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
