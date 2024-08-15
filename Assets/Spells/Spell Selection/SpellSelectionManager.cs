using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SpellSelectionManager : MonoBehaviour
{
    // Fields
    [Header("Affected Character")]
    [SerializeField] private int currentCharacterIndex;

    [Header("Spell List")]
    [SerializeField] private GameObject spellListParent;
    [SerializeField] private GameObject inSetPrefab;
    [SerializeField] private int columnsOfIcons;
    [SerializeField] private Vector2 distanceBetweenIcons;
    
    [Header("Spellbooks")]
    [SerializeField] private byte currentBookIndex;
    [SerializeField] private GameObject spellbookIndexText;

    [Header("Equipped Spell Slots")]
    [SerializeField] private Vector2 spellSlotStart;
    [SerializeField] private float spellSlotSpread;
    [SerializeField] private float slotSnapDistance;
    [HideInInspector] public Vector2[] slotLocations;

    [Header("Equipped Spells")] // Equipped Spells
    [SerializeField] private GameObject equippedSpellsParent;
    [SerializeField] private GameObject equippedSpellPrefab;

    // Properties or whatever
    private Text _bookIndexText;
    private Text BookIndexText
    {
        get
        {
            _bookIndexText = _bookIndexText != null ? _bookIndexText : spellbookIndexText.GetComponent<Text>();
            return _bookIndexText;
        }
    }

    private void Start()
    {
        // Creates empty books for both players
        foreach (CharacterInfo characterInfo in GameSettings.Used.Characters)
        {
            characterInfo.CreateBooks(false);
        }

        CalculateSlotLocations();
        SetBook(0);

    }
    private void CalculateSlotLocations()
    {
        slotLocations = new Vector2[GameSettings.Used.OffensiveSpellSlots + GameSettings.Used.DefensiveSpellSlots];
        for (var i = 0; i < slotLocations.Length; i++)
        {
            slotLocations[i] = spellSlotStart + (i * spellSlotSpread * Vector2.right);
        }
    }

    private void SetBook(byte target)
    {
        currentBookIndex = target;
        BookIndexText.text = (target + 1).ToString();
        UpdateBookDisplays();
    }
    public void NextBook()
    {
        SetBook((byte)((currentBookIndex + 1) % GameSettings.Used.TotalBooks));
    }

    public void PlaceInSlot(EquippableSpell spell)
    {
        // Debug.Log($"Placing {spell} in slot, looping {usedSettings.Characters[currentCharacterIndex].EquippedSpellBooks.Length} times.");
        for (int i = 0; i < GameSettings.Used.TotalSpellSlots; i++)
        {
            if(Vector2.Distance(spell.transform.position, slotLocations[i]) <= slotSnapDistance)
            {
                CharacterInfo.Spellbook book = GameSettings.Used.Characters[currentCharacterIndex].CurrentBook;
                book.SetIndexes[i] = spell.setIndex;
                book.SpellIndexes[i] = spell.spellIndex;
                UpdateBookDisplays();
                return;
            }
        }
    }

    public void CreateSpellObjects(byte selectedSet)
    {
        // Destroy all of the old child objects
        for (byte i = 0; i < spellListParent.transform.childCount; i++)
        {
            Destroy(spellListParent.transform.GetChild(i).gameObject);
        }

        var set = GameSettings.Used.SpellSets[selectedSet];
        for (byte i = 0; i < set.spellsInSet.Length; i++)
        {
            // Instaniates the prefab
            GameObject instantiatedDisplay = Instantiate(inSetPrefab, spellListParent.transform);
            EquippableSpell equippableSpellScript = instantiatedDisplay.GetComponent<EquippableSpell>();

            float x = i % 5;
            float y = Mathf.Floor(i / 5);

            // Set position
            Vector3 displacement = new(x * distanceBetweenIcons.x, y * -distanceBetweenIcons.y, 0);
            instantiatedDisplay.transform.position = spellListParent.transform.position + displacement;

            // Set the object's spell
            equippableSpellScript.setIndex = selectedSet;
            equippableSpellScript.spellIndex = i;

            // Gives the spell a reference to this script
            equippableSpellScript.managerScript = this;
        }
    }
    private void UpdateBookDisplays()
    {
        // Destroy all of the old child objects
        for (int i = 0; i < equippedSpellsParent.transform.childCount; i++)
        {
            Destroy(equippedSpellsParent.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < GameSettings.Used.TotalSpellSlots; i++)
        {
            GameObject instantiatedDisplay = Instantiate(equippedSpellPrefab, equippedSpellsParent.transform);

            CharacterInfo.Spellbook book = GameSettings.Used.Characters[currentCharacterIndex].CurrentBook;
            Sprite icon = SpellManager.GetSpellData(book, (byte)i).Icon;

            instantiatedDisplay.GetComponent<SpriteRenderer>().sprite = icon;
            instantiatedDisplay.transform.position = slotLocations[i];
        }
    }
}
