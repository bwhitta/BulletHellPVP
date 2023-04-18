using UnityEngine;
using UnityEngine.UI;

public class SpellSelectionManager : MonoBehaviour
{
    // Game Settings
    [SerializeField] private GameSettings gameSettings;

    [Space] // Character currently equipping
    [SerializeField] private int currentCharacterIndex;

    [Space] // Icon displays
    [SerializeField] private GameObject inSetPrefab;
    [SerializeField] private GameObject equippedPrefab;
    [SerializeField] private Vector2 distanceBetweenIcons;
    [SerializeField] private int columnsOfIcons;

    [Space] // Spellbooks
    [SerializeField] private int currentBookIndex;
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
        gameSettings.Characters[currentCharacterIndex].CreateBooks();

        CalculateSlotLocations();
        SetBook(0);
    }
    private void CreateBooks()
    {
        for(int i = 0; i < gameSettings.Characters.Length; i++)
        {
            CharacterInfo characterInfo = gameSettings.Characters[i];

            characterInfo.EquippedSpellBooks = new SpellData[gameSettings.TotalBooks][];

            for(int j = 0; j < characterInfo.EquippedSpellBooks.Length; j++)
            {
                characterInfo.EquippedSpellBooks[j] = new SpellData[gameSettings.TotalSpellSlots];
            }
        }
    }
    private void CalculateSlotLocations()
    {
        slotLocations = new Vector2[gameSettings.OffensiveSpellSlots + gameSettings.DefensiveSpellSlots];
        for (var i = 0; i < slotLocations.Length; i++)
        {
            slotLocations[i] = spellSlotStart + (i * spellSlotSpread * Vector2.right);
        }
    }

    private void SetBook(int target)
    {
        currentBookIndex = target;
        BookIndexText.text = (target + 1).ToString();
        UpdateBookDisplays();
    }
    public void NextBook()
    {
        SetBook((currentBookIndex + 1) % gameSettings.TotalBooks);
    }

    public void PlaceInSlot(EquippableSpell spell)
    {
        // Debug.Log($"Placing {spell} in slot, looping {gameSettings.Characters[currentCharacterIndex].EquippedSpellBooks.Length} times.");
        for (int i = 0; i < gameSettings.Characters[currentCharacterIndex].EquippedSpellBooks[currentBookIndex].Length; i++)
        {
            if(Vector2.Distance(spell.transform.position, slotLocations[i]) <= slotSnapDistance)
            {
                gameSettings.Characters[currentCharacterIndex].EquippedSpellBooks[currentBookIndex][i] = spell.spellData;
                UpdateBookDisplays();
                return;
            }
        }
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
            GameObject instantiatedDisplay = Instantiate(inSetPrefab, transform);

            float x = i % 5;
            float y = Mathf.Floor(i / 5);

            Vector3 displacement = new(x * distanceBetweenIcons.x, y * -distanceBetweenIcons.y, 0);
            instantiatedDisplay.transform.position = transform.position + displacement;

            //Set the object's spell
            instantiatedDisplay.GetComponent<EquippableSpell>().spellData = selectedSet.spellsInSet[i];
        }
    }
    private void UpdateBookDisplays()
    {
        // Destroy all of the old child objects
        for (int i = 0; i < equippedSpellArea.transform.childCount; i++)
        {
            Destroy(equippedSpellArea.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < gameSettings.TotalSpellSlots; i++)
        {
            if (gameSettings.Characters[currentCharacterIndex].EquippedSpellBooks[currentBookIndex][i] == null)
            {
                continue;
            }
            GameObject instantiatedDisplay = Instantiate(equippedPrefab, equippedSpellArea.transform);
            instantiatedDisplay.GetComponent<SpriteRenderer>().sprite = gameSettings.Characters[currentCharacterIndex].EquippedSpellBooks[currentBookIndex][i].Icon;
            instantiatedDisplay.transform.position = slotLocations[i];
        }
    }
}
