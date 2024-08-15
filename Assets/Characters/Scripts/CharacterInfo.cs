using UnityEngine;

[CreateAssetMenu(menuName = "Character Information")]
public class CharacterInfo : ScriptableObject
{
    [Space(25)] // Opponent info
    public CharacterInfo OpponentCharacterInfo;

    [Space(25)] // Tags
    public string CharacterAndSortingTag;
    public string MainCanvasTag;

    [Space(25)] // Controls
    public string InputMapName;
    public string MovementActionName;
    public string NextBookActionName;
    public string CastingActionName;

    [Space(25)] // Movement
    public Vector2 CharacterStartLocation;

    [Space(25)] // Animation
    public string AnimatorTreeParameterX;
    public string AnimatorTreeParameterY;

    [Space(25)] // Equipped Spells
    public byte CurrentBookIndex;
    [SerializeField] private bool overrideFirstBook;
    [SerializeField] private byte[] overrideSetIndexes, overrideSpellIndexes;
    private Spellbook[] EquippedBooks;
    public Spellbook CurrentBook => EquippedBooks[CurrentBookIndex];
    public class Spellbook
    {
        public byte[] SetIndexes;
        public byte[] SpellIndexes;
    }

    [Space(25)] // Positioning
    public Vector2 OpponentAreaCenter;
    public Vector2 SpellbookPosition;
    public Vector2 SpellbookScale;

    public void CreateBooks(bool allowOverride = true)
    {
        if (EquippedBooks != null)
        {
            return;
        }

        Debug.Log($"Equipped books found null, creating new spellbooks");
        EquippedBooks = new Spellbook[GameSettings.Used.TotalBooks];
        
        for (int i = 0; i < EquippedBooks.Length; i++)
        {
            if(i == 0 && overrideFirstBook)
            {
                EquippedBooks[i] = new()
                {
                    SetIndexes = overrideSetIndexes,
                    SpellIndexes = overrideSpellIndexes
                };
                continue;
            }
            EquippedBooks[i] = new()
            {
                SetIndexes = new byte[GameSettings.Used.TotalSpellSlots],
                SpellIndexes = new byte[GameSettings.Used.TotalSpellSlots]
            };
        }
    }

    #region TaggedObjectReferences
    // Health and Mana
    private BarLogic _healthBar;
    private BarLogic _manaBar;
    public BarLogic HealthBar
    {
        get
        {
            if (_healthBar == null)
                _healthBar = TaggedObjectWithType<BarLogic>(BarLogic.Stats.health).GetComponent<BarLogic>();
            return _healthBar;
        }
        set
        {
            _healthBar = value;
        }
    }
    public BarLogic ManaBar
    {
        get
        {
            if (_manaBar == null)
                _manaBar = TaggedObjectWithType<BarLogic>(BarLogic.Stats.mana).GetComponent<BarLogic>();
            return _manaBar;
        }
        set
        {
            _manaBar = value;
        }
    }
    // Character Object
    private GameObject _characterObject;
    public GameObject CharacterObject
    {
        get
        {
            if (_characterObject == null)
            {
                _characterObject = TaggedObjectWithType<CharacterStats>();
            }
            return _characterObject;
        }
    }
    // Character stats script
    private CharacterStats _stats;
    public CharacterStats Stats
    {
        get
        {
            if (CharacterObject == null)
            {
                Debug.LogWarning("CharacterObject null, setting CharacterStats to null");
                _stats = null;
            }
            else if (_stats == null)
            {
                _stats = CharacterObject.GetComponent<CharacterStats>();
            }
            return _stats;
        }
    }
    
    // Spell manager
    private SpellManager _spellManagerScript;
    public SpellManager SpellManagerScript
    {
        get
        {
            if (_spellManagerScript == null)
            {
                GameObject cursorObject = TaggedObjectWithType<SpellManager>();
                if (cursorObject != null)
                {
                    _spellManagerScript = cursorObject.GetComponent<SpellManager>();
                }
                else
                {
                    Debug.Log($"No spell manager found!");
                    _spellManagerScript = null;
                }
            }
            return _spellManagerScript;
        }
    }

    // Cursor logic
    private CursorLogic _cursorLogicScript;
    public CursorLogic CursorLogicScript
    {
        get
        {
            if (_cursorLogicScript == null)
            {
                GameObject spellManagerObject = TaggedObjectWithType<SpellManager>();
                if (spellManagerObject != null)
                {
                    _cursorLogicScript = spellManagerObject.GetComponent<CursorLogic>();
                }
                else
                {
                    Debug.Log($"No spell manager found!");
                    _cursorLogicScript = null;
                }
            }
            return _cursorLogicScript;
        }
    }
    

    // Spellbook script
    [HideInInspector] public SpellbookLogic _spellbookLogicScript;
    public SpellbookLogic SpellbookLogicScript
    {
        get
        {
            if(_spellbookLogicScript == null)
            {
                GameObject spellbookObject = TaggedObjectWithType<SpellbookLogic>();
                if(spellbookObject != null)
                {
                    _spellbookLogicScript = spellbookObject.GetComponent<SpellbookLogic>();
                }
                else
                {
                    Debug.LogWarning($"spellbookLogicScript is null");
                }
            }
            return _spellbookLogicScript;
        }
        set
        {
            _spellbookLogicScript = value;
        }
    }

    /// <summary> Find a gameobject with a specific type from a tag </summary>
    private GameObject TaggedObjectWithType<objectType>(BarLogic.Stats? barStat = null)
    {
        // Find the list of objects that share a tag with the character
        GameObject[] tagged = GameObject.FindGameObjectsWithTag(CharacterAndSortingTag);
        bool findingBarStat = false;
        if(barStat != null)
        {
            findingBarStat = true;
        }

        // Find the object in the list
        for (int i = 0; i < tagged.Length; i++)
        {
            // Check if it is a bar with with tag
            if (findingBarStat && tagged[i].GetComponent<BarLogic>() != null)
            {
                // Checks if the bar has the correct tag
                if (tagged[i].GetComponent<BarLogic>().statToModify == barStat)
                {
                    return tagged[i];
                }
            }
            // Checks if tagged[i] has the right objectType attatched
            else if (tagged[i].GetComponent(typeof(objectType)))
            {
                
                return tagged[i];
            }
        }
        return null;
    }
    #endregion ObjectReferences
}
