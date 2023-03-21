using UnityEngine;
using UnityEngine.UI;

public class SetManager : MonoBehaviour
{
    [Header("Sets")]
    [SerializeField] private GameSettings gameSettings;
    
    [Header("Display")]
    [SerializeField] private GameObject setDisplayPrefab;
    [SerializeField] private float distanceBetweenIcons;
    
    [Header("Selection")]
    private SpellSetInfo selectedSet;
    [SerializeField] private GameObject selectionDisplay;

    [Header("Spells")]
    [SerializeField] private SpellSelectionManager spellSelectionManager;

    private void Start()
    {
        if (gameSettings.SpellSets.Length == 0)
            Debug.LogWarning("No sets given to set manager");

        SelectSet(0);

        DisplaySetInfo();
    }

    private void DisplaySetInfo()
    {
        // Destroy all of the old child objects
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < gameSettings.SpellSets.Length; i++)
        {
            // Instaniates the child
            GameObject childObject = Instantiate(setDisplayPrefab, transform);
            childObject.transform.position = transform.position + (distanceBetweenIcons * i * Vector3.down);
            
            //Set the child's sprite
            childObject.GetComponent<SpriteRenderer>().sprite = gameSettings.SpellSets[i].SetSprite;

            // Add a listener to the button
            int localIndex = i;
            childObject.GetComponentInChildren<Button>().onClick.AddListener(delegate { SelectSet(localIndex); });
        }
    }

    private void SelectSet(int index)
    {
        if (index >= gameSettings.SpellSets.Length)
        {
            Debug.LogWarning($"Index out of bounds (index {index} is greater than length {gameSettings.SpellSets.Length}");
            return;
        }
        else if (selectionDisplay == null)
        {
            Debug.LogWarning("Selection display null.");
            return;
        }

        selectedSet = gameSettings.SpellSets[index];

        spellSelectionManager.CreateSpellObjects(selectedSet);

        selectionDisplay.transform.position = transform.position + (distanceBetweenIcons * index * Vector3.down);
    }
}
