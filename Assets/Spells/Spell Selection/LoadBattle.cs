using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadBattle : MonoBehaviour
{
    [SerializeField] private string gameplayScene;

    void Start()
    {
        // When online, only show the start button to the host
        if (MultiplayerManager.IsOnline)
        {
            LobbyManager lobbyManager = FindObjectsOfType<LobbyManager>()[0];
            if (lobbyManager == null)
            {
                Debug.LogError("lobby null!");
            }

            gameObject.SetActive(lobbyManager.IsLobbyHost);
        }
    }

    public void LoadGameplayScene()
    {
        // Uses the LobbyManager to start the game when playing online, otherwise just loads the scene.
        if (MultiplayerManager.IsOnline)
        {
            LobbyManager lobbyManager = FindObjectsOfType<LobbyManager>()[0];
            
            Debug.Log($"Starting game online");
            lobbyManager.HostStartGame();
        }
        else
        {
            SceneManager.LoadScene(gameplayScene);
            Debug.Log($"Starting local game");
        }
    }
}
