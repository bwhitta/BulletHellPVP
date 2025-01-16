using UnityEngine;
using UnityEngine.UI;

public class SpellSelectionManager : MonoBehaviour
{
    // Fields
    [SerializeField] private GameSettings defaultGameSettings;

    [Header("Spellbooks")]
    [SerializeField] private byte currentBookIndex;
    [SerializeField] private GameObject spellbookIndexText;

    [Header("Equipped Spell Slot Locations")]
    [SerializeField] private Vector2 spellSlotStart;
    [SerializeField] private float spellSlotSpread;
    [SerializeField] private float slotSnapDistance;
    [HideInInspector] public Vector2[] slotPositions;

    [Header("Equipped Spell Displays")]
    [SerializeField] private GameObject equippedSpellsParent;
    [SerializeField] private GameObject equippedSpellPrefab;

    private LobbyManager lobbyManager;

    // Properties
    private Text _bookIndexText;
    private Text BookIndexText
    {
        get
        {
            _bookIndexText = _bookIndexText != null ? _bookIndexText : spellbookIndexText.GetComponent<Text>();
            return _bookIndexText;
        }
    }

    // At least for now, in online play the lobby host is always on the left.
    private byte _currentCharacterIndex;
    private byte CurrentCharacterIndex
    {
        get
        {
            if (MultiplayerManager.IsOnline)
            {
                if (lobbyManager.IsLobbyHost) _currentCharacterIndex = 0;
                else _currentCharacterIndex = 1;
            }
            return _currentCharacterIndex;
        }
        set => _currentCharacterIndex = value;
    }

    private Spellbook CurrentEditedBook
    {
        get => SpellbookLogic.EquippedBooks[CurrentCharacterIndex][currentBookIndex];
        set => SpellbookLogic.EquippedBooks[CurrentCharacterIndex][currentBookIndex] = value;
    }

    // Methods
    private void Awake()
    {
        GameSettings.Used = defaultGameSettings;
    }
    private void Start()
    {
        if (MultiplayerManager.IsOnline)
        {
            lobbyManager = FindObjectOfType<LobbyManager>();
        }

        // what happens if I start the game when in the battle scene? how does this get started?
        SpellbookLogic.EquippedBooks = new Spellbook[GameSettings.Used.MaxCharacters][];
        for (int i = 0; i < GameSettings.Used.MaxCharacters; i++)
        {
            SpellbookLogic.EquippedBooks[i] = Spellbook.CreateBooks(GameSettings.Used.TotalBooks, GameSettings.Used.SpellSlots);
        }

        slotPositions = CalculateSlotPositions();
        SetBook(0);

        // Local methods
        Vector2[] CalculateSlotPositions()
        {
            Vector2[] locations = new Vector2[GameSettings.Used.SpellSlots];
            for (var i = 0; i < locations.Length; i++)
            {
                locations[i] = spellSlotStart + (i * spellSlotSpread * Vector2.right);
            }
            return locations;
        }
    }
    private void SetBook(byte target)
    {
        Debug.Log($"setting book to {target}");
        currentBookIndex = target;
        BookIndexText.text = (target + 1).ToString();
        UpdateBookDisplays();
    }
    public void NextBook()
    {
        SetBook((byte)((currentBookIndex + 1) % GameSettings.Used.TotalBooks));
    }
    
    // surely I could re-work this so it doesn't delete and re-spawn all children every time I change something, right?
    private void UpdateBookDisplays()
    {
        // Destroy all of the old child objects
        foreach (Transform child in equippedSpellsParent.transform)
        {
            Destroy(child.gameObject);
        }

        for (byte i = 0; i < GameSettings.Used.SpellSlots; i++)
        {
            GameObject instantiatedDisplay = Instantiate(equippedSpellPrefab, equippedSpellsParent.transform);

            Spellbook book = SpellbookLogic.EquippedBooks[CurrentCharacterIndex][currentBookIndex];
            Sprite icon = book.SpellInSlot(i).Icon;

            instantiatedDisplay.GetComponent<SpriteRenderer>().sprite = icon;
            instantiatedDisplay.transform.position = slotPositions[i];
        }
    }
    public void PlaceInSlot(EquippableSpell spell)
    {
        // Figure out which slot the spell fits in
        for (byte i = 0; i < GameSettings.Used.SpellSlots; i++)
        {
            if (Vector2.Distance(spell.transform.position, slotPositions[i]) <= slotSnapDistance)
            {
                CurrentEditedBook.SetIndexes[i] = spell.setIndex;
                CurrentEditedBook.SpellIndexes[i] = spell.spellIndex;
                UpdateBookDisplays();
                return;
            }
        }
    }
}
