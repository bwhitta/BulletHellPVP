using Unity.Netcode;
using UnityEngine;

public class MultiplayerManager : MonoBehaviour
{
    public enum MultiplayerTypes { Local, OnlineHost, OnlineClient }
    public static MultiplayerTypes multiplayerType;
    [SerializeField] private GameObject playerPrefab;
    private void Awake()
    {
        Debug.Log($"Multiplayer type: {multiplayerType}");
        switch (multiplayerType)
        {
            case MultiplayerTypes.OnlineClient:
                NetworkManager.Singleton.StartClient();
                break;
            case MultiplayerTypes.OnlineHost:
                NetworkManager.Singleton.StartHost();
                break;
            default:
                Instantiate(playerPrefab);
                Instantiate(playerPrefab);
                break;
        }
    }

    private void Update()
    {
        SubmitNewPosition();
    }

    static void SubmitNewPosition()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Player is server, host, or local player. Continuing position change.");
        }
        else
        {
            Debug.Log("Not server, aborting position change.");
            return;
        }

        if (!NetworkManager.Singleton.IsClient)
        {
            foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
                NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<PlayerNetworking>().Move();
        }
        else
        {
            NetworkObject playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            PlayerNetworking player = playerObject.GetComponent<PlayerNetworking>();
            player.Move();
        }
    }

}
