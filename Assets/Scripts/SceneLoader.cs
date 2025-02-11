using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Loads a scene
    public static void LoadScene (string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    public void LoadScene (int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
