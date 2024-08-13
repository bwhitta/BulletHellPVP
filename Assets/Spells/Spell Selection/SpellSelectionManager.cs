using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SpellSelectionManager : MonoBehaviour
{
    [Space] // Character currently equipping
    [SerializeField] private int currentCharacterIndex;

    [Space] // Icon displays
    [SerializeField] private GameObject inSetPrefab;
    [SerializeField] private GameObject equippedPrefab;
    [SerializeField] private Vector2 distanceBetweenIcons;
    [SerializeField] private int columnsOfIcons;

    [Space] // EquippedBooks
    [SerializeField] private byte currentBookIndex;
    [SerializeField] private GameObject spellbookIndexText;
    private Text _bookIndexText;
    private Text BookIndexText
    {
        get
        {
            _bookIndexText = _bookIndexText != null ? _bookIndexText : spellbookIndexText.GetComponent<Text>();
            return _bookIndexText;
        }
    }

    [Space][Space] // Slots
    [SerializeField] private Vector2 spellSlotStart;
    [SerializeField] private float spellSlotSpread;
    [SerializeField] private float slotSnapDistance;
    [HideInInspector] public Vector2[] slotLocations;

    [Space] // Equipped Spells
    [SerializeField] private GameObject equippedSpellArea;

    private void Start()
    {
        CreateBooks();
        GameSettings.Used.Characters[currentCharacterIndex].CreateBooks();

        CalculateSlotLocations();
        SetBook(0);
    }
    private void CreateBooks()
    {
        foreach (CharacterInfo characterInfo in GameSettings.Used.Characters)
        {
            characterInfo.CreateBooks();
        }
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
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        var set = GameSettings.Used.SpellSets[selectedSet];
        for (int i = 0; i < set.spellsInSet.Length; i++)
        {
            // Instaniates the prefab
            GameObject instantiatedDisplay = Instantiate(inSetPrefab, transform);

            float x = i % 5;
            float y = Mathf.Floor(i / 5);

            Vector3 displacement = new(x * distanceBetweenIcons.x, y * -distanceBetweenIcons.y, 0);
            instantiatedDisplay.transform.position = transform.position + displacement;

            //Set the object's spell
            //instantiatedDisplay.GetComponent<EquippableSpell>().spellData = selectedSet.spellsInSet[i];
            instantiatedDisplay.GetComponent<EquippableSpell>().setIndex = selectedSet;
            instantiatedDisplay.GetComponent<EquippableSpell>().spellIndex = (byte)i;
        }
    }
    private void UpdateBookDisplays()
    {
        // Destroy all of the old child objects
        for (int i = 0; i < equippedSpellArea.transform.childCount; i++)
        {
            Destroy(equippedSpellArea.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < GameSettings.Used.TotalSpellSlots; i++)
        {
            GameObject instantiatedDisplay = Instantiate(equippedPrefab, equippedSpellArea.transform);

            CharacterInfo.Spellbook book = GameSettings.Used.Characters[currentCharacterIndex].CurrentBook;
            Sprite icon = SpellManager.GetSpellData(book, (byte)i).Icon;

            instantiatedDisplay.GetComponent<SpriteRenderer>().sprite = icon;
            instantiatedDisplay.transform.position = slotLocations[i];
        }
    }
}
