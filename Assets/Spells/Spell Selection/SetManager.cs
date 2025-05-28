using UnityEngine;
using UnityEngine.UI;

public class SetManager : MonoBehaviour
{
    [Header("Sets List")]
    [SerializeField] private GameObject selectedSetIndicator;
    [SerializeField] private GameObject spellSetParent;
    [SerializeField] private GameObject spellSetPrefab;

    private byte selectedSet;

    [Header("Spell List")]
    [SerializeField] private GameObject listedSpellsParent;
    [SerializeField] private GameObject listedSpellPrefab;
    [SerializeField] private float distanceBetweenIcons;

    [Header("Spells")]
    private SpellSelectionManager spellSelectionManager;


    private void Start()
    {
        spellSelectionManager = GetComponent<SpellSelectionManager>();

        if (GameSettings.Used.SpellSets.Length == 0)
            Debug.LogWarning("No sets given to set manager");

        SelectSet(0);

        ListSets();
    }
     
    private void ListSets()
    {
        // Destroy all previously displayed sets
        foreach (Transform child in spellSetParent.transform)
        {
            Destroy(child.gameObject);
        }

        for (byte i = 0; i < GameSettings.Used.SpellSets.Length; i++)
        {
            // Instaniates the set, then set its location and icon
            GameObject spellSetDisplay = Instantiate(spellSetPrefab, spellSetParent.transform);
            spellSetDisplay.transform.position = spellSetParent.transform.position + distanceBetweenIcons * i * Vector3.down;
            spellSetDisplay.GetComponent<Image>().sprite = GameSettings.Used.SpellSets[i].SetSprite;

            // Clicking the set will select it
            byte setIndex = i; // i is not considered local to the loop, so when SelectSet is called it uses the highest value of i (which is 1 higher than the last set)
            spellSetDisplay.GetComponentInChildren<Button>().onClick.AddListener(delegate { SelectSet(setIndex); });
        }
    }
    public void SelectSet(byte index)
    {
        if (index >= GameSettings.Used.SpellSets.Length)
        {
            Debug.LogWarning($"Index out of bounds (index {index} is greater than length {GameSettings.Used.SpellSets.Length}");
            return;
        }
        else if (selectedSetIndicator == null)
        {
            Debug.LogWarning("Selected set indicator is null! You probably forgot to set a reference.", this);
            return;
        }
        
        // Store the currently displayed set's index, and show the spells within that set. It's not necessary to store the index but it could be useful in the future.
        selectedSet = index;
        DisplaySpellsInSet(selectedSet);

        // Move the selection display
        var parentPosition = selectedSetIndicator.transform.parent.position;
        selectedSetIndicator.transform.position = parentPosition + (distanceBetweenIcons * index * Vector3.down);
    }
    private void DisplaySpellsInSet(byte selectedSet)
    {
        // Destroy all of the old child objects
        foreach (Transform child in listedSpellsParent.transform)
        {
            Destroy(child.gameObject);
        }

        var set = GameSettings.Used.SpellSets[selectedSet];
        for (byte i = 0; i < set.spellsInSet.Length; i++)
        {
            // Instaniates the prefab
            GameObject instantiatedDisplay = Instantiate(listedSpellPrefab, listedSpellsParent.transform);
            EquippableSpell equippableSpellScript = instantiatedDisplay.GetComponent<EquippableSpell>();

            float x = i % 5;
            float y = Mathf.Floor(i / 5);

            // Set position
            Vector3 displacement = new(x * distanceBetweenIcons, y * -distanceBetweenIcons);
            instantiatedDisplay.transform.position = listedSpellsParent.transform.position + displacement;

            // Set the object's spell
            equippableSpellScript.setIndex = selectedSet;
            equippableSpellScript.spellIndex = i;

            // Gives the spell a reference to this script
            equippableSpellScript.managerScript = spellSelectionManager;
        }
    }
    
}
