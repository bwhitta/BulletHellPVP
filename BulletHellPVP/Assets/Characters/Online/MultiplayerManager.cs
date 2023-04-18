using Unity.Netcode;
using UnityEngine;

public class MultiplayerManager : MonoBehaviour
{
    public enum MultiplayerTypes { Local, OnlineHost, OnlineClient }
    public static MultiplayerTypes multiplayerType;
    [SerializeField] private GameObject characterPrefab;
    private void Start()
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
                Instantiate(characterPrefab);
                Instantiate(characterPrefab);
                break;
        }
    }

    private void Update()
    {
        SubmitNewPosition();
    }

    static void SubmitNewPosition()
    {
        if (multiplayerType == MultiplayerTypes.Local)
        {
            // Debug.Log("Skipping online location submitting (game is local).");
            return;
        }
        /*
        else if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Character is server or host. Continuing position change.");
        }
        else
        {
            Debug.Log("Game is not a server, aborting position change.");
            return;
        }*/
        if (NetworkManager.Singleton.IsClient == false)
        {
            foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
                NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<PlayerNetworking>().Move();
        }
        else
        {
            NetworkObject characterObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            PlayerNetworking character = characterObject.GetComponent<PlayerNetworking>();
            character.Move();
            Debug.Log($"Character object: {characterObject} character: {character}");
        }
    }

}
