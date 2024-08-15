using UnityEngine;
using UnityEngine.UI;

public class SetManager : MonoBehaviour
{
    [SerializeField] private GameSettings usedSettings;

    [Header("Display")]
    [SerializeField] private GameObject setDisplayParent;
    [SerializeField] private GameObject setDisplayPrefab;
    [SerializeField] private float distanceBetweenIcons;
    
    [Header("Selection")]
    [SerializeField] private GameObject selectionDisplay;
    private byte selectedSet;

    [Header("Spells")]
    [SerializeField] private SpellSelectionManager spellSelectionManager;

    private void Awake()
    {
        GameSettings.Used = usedSettings;
    }

    private void Start()
    {
        if (usedSettings.SpellSets.Length == 0)
            Debug.LogWarning("No sets given to set manager");

        SelectSet(0);

        DisplaySetInfo();
    }

    private void DisplaySetInfo()
    {
        // Destroy all of the old child objects
        for (int i = 0; i < setDisplayParent.transform.childCount; i++)
        {
            Destroy(setDisplayParent.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < usedSettings.SpellSets.Length; i++)
        {
            // Instaniates the child
            GameObject childObject = Instantiate(setDisplayPrefab, setDisplayParent.transform);
            
            // Sets the location
            childObject.transform.position = setDisplayParent.transform.position + (distanceBetweenIcons * i * Vector3.down);
            
            //Set the child's sprite
            childObject.GetComponent<SpriteRenderer>().sprite = usedSettings.SpellSets[i].SetSprite;

            // Add a listener to the button
            byte localIndex = (byte)i;
            childObject.GetComponentInChildren<Button>().onClick.AddListener(delegate { SelectSet(localIndex); });
        }
    }

    private void SelectSet(byte index)
    {
        if (index >= usedSettings.SpellSets.Length)
        {
            Debug.LogWarning($"Index out of bounds (index {index} is greater than length {usedSettings.SpellSets.Length}");
            return;
        }
        else if (selectionDisplay == null)
        {
            Debug.LogWarning("Selection display null.");
            return;
        }
        
        // Store the currently displayed set's index, and show the spells within that set. It's not necessary to store the index but it could be useful in the future.
        selectedSet = index;
        spellSelectionManager.CreateSpellObjects(selectedSet);

        // Move the selection display
        var parentPosition = selectionDisplay.transform.parent.position;
        selectionDisplay.transform.position = parentPosition + (distanceBetweenIcons * index * Vector3.down);
    }
}
