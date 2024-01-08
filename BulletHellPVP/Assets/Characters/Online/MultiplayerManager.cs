using Unity.Netcode;
using UnityEngine;

public class MultiplayerManager : MonoBehaviour
{
    public enum MultiplayerTypes { Local, OnlineHost, OnlineClient }
    public static MultiplayerTypes multiplayerType;
    [SerializeField] private GameObject characterPrefab;
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
            case MultiplayerTypes.OnlineClient:
                NetworkManager.Singleton.StartClient();
                break;
            case MultiplayerTypes.OnlineHost:
                NetworkManager.Singleton.StartHost();
                break;
            case MultiplayerTypes.Local:
                Instantiate(characterPrefab);
                Instantiate(characterPrefab);
                break;
            default:
                multiplayerType = MultiplayerTypes.Local;
                Instantiate(characterPrefab);
                Instantiate(characterPrefab);
                break;
        }
    }
}
