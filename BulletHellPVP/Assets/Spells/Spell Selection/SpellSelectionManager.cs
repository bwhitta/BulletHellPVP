using System.Data;
using UnityEngine;

public class SpellSelectionManager : MonoBehaviour
{
    // Game Settings
    [SerializeField] private GameSettings gameSettings;

    [Space] // Character currently equipping
    [SerializeField] private int currentCharacterIndex;

    [Space] // Icon displays
    [SerializeField] private GameObject displayPrefab;
    [SerializeField] private Vector2 distanceBetweenIcons;
    [SerializeField] private int columnsOfIcons;

    [Space] // Slots
    public GameObject equippedSpellArea;
    [SerializeField] private Vector2 spellSlotStart;
    [SerializeField] private float spellSlotSpread;
    [SerializeField] private float slotSnapDistance;
    [HideInInspector] public Vector2[] slotLocations;
    private EquippableSpell[] currentlyInSlots;

    private void Start()
    {
        CalculateSlotLocations();
        currentlyInSlots = new EquippableSpell[slotLocations.Length];
    }
    private void CalculateSlotLocations()
    {
        slotLocations = new Vector2[gameSettings.OffensiveSpellSlots + gameSettings.DefensiveSpellSlots];
        for (var i = 0; i < slotLocations.Length; i++)
        {
            slotLocations[i] = spellSlotStart + (i * spellSlotSpread * Vector2.right);
        }
    }

    public bool PlaceInSlot(EquippableSpell spell)
    {
        Debug.Log($"Placing {spell} in slot, looping {gameSettings.Characters[currentCharacterIndex].EquippedSpells.Length} times.");
        for (int i = 0; i < gameSettings.Characters[currentCharacterIndex].EquippedSpells.Length; i++)
        {
            Debug.Log($"Iteration {i}, distance {Vector2.Distance(spell.transform.position, slotLocations[i])}");
            if(Vector2.Distance(spell.transform.position, slotLocations[i]) <= slotSnapDistance)
            {
                Debug.Log($"Fits in slot at {slotLocations[i]}");

                gameSettings.Characters[currentCharacterIndex].EquippedSpells[i] = spell.spellData;
                spell.transform.position = slotLocations[i];
                
                if (currentlyInSlots[i] != null)
                    Destroy(currentlyInSlots[i].gameObject);
    
                currentlyInSlots[i] = spell;
                return true;
            }
        }
        return false;
    }

    public void CreateSpellObjects(SpellSetInfo selectedSet)
    {
        // Destroy all of the old child objects
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < selectedSet.spellsInSet.Length; i++)
        {
            // Instaniates the prefab
            GameObject instantiatedDisplay = Instantiate(displayPrefab, transform);

            float x = i % 5;
            float y = Mathf.Floor(i / 5);

            Vector3 displacement = new(x * distanceBetweenIcons.x, y * -distanceBetweenIcons.y, 0);
            instantiatedDisplay.transform.position = transform.position + displacement;

            //Set the object's spell
            instantiatedDisplay.GetComponent<EquippableSpell>().spellData = selectedSet.spellsInSet[i];
        }
    }
}
