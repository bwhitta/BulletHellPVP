using UnityEngine;
using UnityEngine.UI;

public class SetManager : MonoBehaviour
{
    [Header("Sets")]
    [SerializeField] private GameSettings usedSettings;
    
    [Header("Display")]
    [SerializeField] private GameObject setDisplayPrefab;
    [SerializeField] private float distanceBetweenIcons;
    
    [Header("Selection")]
    private byte selectedSet;
    [SerializeField] private GameObject selectionDisplay;

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
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < usedSettings.SpellSets.Length; i++)
        {
            // Instaniates the child
            GameObject childObject = Instantiate(setDisplayPrefab, transform);
            childObject.transform.position = transform.position + (distanceBetweenIcons * i * Vector3.down);
            
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

        selectedSet = index;
        spellSelectionManager.CreateSpellObjects(selectedSet);

        selectionDisplay.transform.position = transform.position + (distanceBetweenIcons * index * Vector3.down);
    }
}
