using Unity.Netcode;
using UnityEngine;

public class MultiplayerManager : MonoBehaviour
{
    public enum MultiplayerTypes { Local, OnlineHost, OnlineClient }
    public static MultiplayerTypes multiplayerType;
    [SerializeField] private GameObject characterPrefab;

    private void Start()
    {
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
}
