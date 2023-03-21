using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButtonLogic : MonoBehaviour
{
    public void SetScene(string SceneToSet)
    {
        SceneManager.LoadScene(SceneToSet);
    }
}
