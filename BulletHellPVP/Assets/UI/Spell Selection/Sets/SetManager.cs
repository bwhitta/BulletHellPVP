using UnityEngine;
using UnityEngine.UI;

public class SetManager : MonoBehaviour
{
    [Header("Sets")]
    [SerializeField] private SpellSetInfo[] allSets;
    
    [Header("Display")]
    [SerializeField] private GameObject setDisplayPrefab;
    [SerializeField] private float distanceBetweenIcons;
    
    [Header("Selection")]
    private SpellSetInfo selectedSet;
    [SerializeField] private GameObject selectionDisplay;

    [Header("Spells")]
    [SerializeField] private GameObject spellSelectionManager;

    private void Start()
    {
        if (allSets.Length == 0)
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

        for (int i = 0; i < allSets.Length; i++)
        {
            // Instaniates the child
            GameObject childObject = Instantiate(setDisplayPrefab, transform);
            childObject.transform.position = transform.position + (distanceBetweenIcons * i * Vector3.down);
            
            //Set the child's sprite
            childObject.GetComponent<SpriteRenderer>().sprite = allSets[i].SetSprite;

            // Add a listener to the button
            int localIndex = i;
            childObject.GetComponentInChildren<Button>().onClick.AddListener(delegate { SelectSet(localIndex); });
        }
    }

    private void SelectSet(int index)
    {
        if (index >= allSets.Length)
        {
            Debug.LogWarning($"Index out of bounds (index {index} is greater than length {allSets.Length}");
            return;
        }
        else if (selectionDisplay == null)
        {
            Debug.LogWarning("Selection display null.");
            return;
        }

        selectedSet = allSets[index];

        spellSelectionManager.GetComponent<SpellSelectionManager>().CreateSpellObjects(selectedSet);

        selectionDisplay.transform.position = transform.position + (distanceBetweenIcons * index * Vector3.down);
    }
}
