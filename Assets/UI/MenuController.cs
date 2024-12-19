using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private string[] MenuNames; // The names of each menu
    [SerializeField] private GameObject[] MenuCanvasParents; // The parents for each menu's canvas elements
    [SerializeField] private GameObject[] MenuNonCanvasParents; // The parents for each menu's non-canvas elements
    [SerializeField] private string StartingMenu; // The menu the game opens to
    [SerializeField] private string spellSelectionScene, lobbySetupScene;

    private void Start()
    {
        // The following If/Else statements check to make sure the variables are properly filled
        if (MenuCanvasParents.Length != MenuNames.Length)
        {
            Debug.LogWarning($"The number of menu canvas parents is different from the number of menus. ({MenuCanvasParents.Length} and {MenuNames.Length})");
        }
        else if (MenuNonCanvasParents.Length != MenuNames.Length)
        {
            Debug.LogWarning($"The number of non-canvas menu parents is different from the number of menus. ({MenuNonCanvasParents.Length} and {MenuNames.Length})");
        }
        else
        {
            //Debug.Log(MenuNames.Length.ToString() + " menus loaded sucessfully.");
            OpenMenu("MainMenu");
        }
    }

    /// <summary>
    /// Disables all menus.
    /// </summary>
    /// <param name="ExclusionList">(Optional) An array of menus to not hide.</param>
    public void DisableAllMenus(string[] ExclusionList = default)
    {
        for (int i = 0; i < MenuNames.Length; i++)
        {
            if (ExclusionList == null || ExclusionList.Contains(MenuNames[i]) == false)
            {
                MenuCanvasParents[i].SetActive(false);
                MenuNonCanvasParents[i].SetActive(false);
                // Debug.Log(MenuNames[i] + " disabled");
            }
        }
    }
    
    /// <summary>
    /// Enables a menu.
    /// </summary>
    /// <param name="MenuToEnable"> The menu to enable. </param>
    public void EnableMenu(string MenuToEnable)
    {
        int IndexCoords;
        if (MenuNames.Contains(MenuToEnable) == false)
        {
            Debug.LogWarning("The menu to enable, '" + MenuToEnable + "', is not listed. Menu enabling cancelled.");
            return;
        }

        IndexCoords = Array.IndexOf(MenuNames, MenuToEnable); // Sets IndexCoords to where the MenuToEnable is in the three lists.

        MenuCanvasParents[IndexCoords].SetActive(true);
        MenuNonCanvasParents[IndexCoords].SetActive(true);

        //Debug.Log(MenuNames[IndexCoords] + " enabled");
    }

    /// <summary>
    /// Closes all menus and simultaneously opens another menu.
    /// </summary>
    /// <param name="MenuToOpen">The menu to open after closing all other menus.</param>
    public void OpenMenu(string MenuToOpen)
    {
        DisableAllMenus();
        EnableMenu(MenuToOpen);
    }
    
    // Called from the buttons on the main menu
    public void StartLocal() 
    {
        MultiplayerManager.IsOnline = false;
        SceneManager.LoadScene(spellSelectionScene);
    }
    public void StartOnline()
    {
        MultiplayerManager.IsOnline = true;
        SceneManager.LoadScene(lobbySetupScene);
    }
}
