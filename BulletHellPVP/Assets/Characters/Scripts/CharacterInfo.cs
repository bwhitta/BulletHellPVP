using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngineInternal.XR.WSA;

[CreateAssetMenu(menuName = "Character Information")]
public class CharacterInfo : ScriptableObject
{
    // Game Settings
    public GameSettings gameSettings;
    [Space(25)]// Opponent info
    public CharacterInfo OpponentCharacterInfo;
    [Space(25)] // Character stats
    public BasicStats DefaultStats;
    [Space(25)] // The tag used for all objects relating to this character
    public string CharacterAndSortingTag;
    [Space(25)] // Movement
    public Vector2 CharacterStartLocation;
    public string InputMapName;
    public string MovementActionName;
    public string SpellbookSelectionActionName;
    [Space(25)] // Animation
    public string AnimatorTreeParameterX;
    public string AnimatorTreeParameterY;
    [Space(25)] //Equipped Spells
    [SerializeField] private SpellData[] _equippedSpells;
    public SpellData[] EquippedSpells
    {
        get
        {
            if (_equippedSpells.Length < gameSettings.TotalSpellSlots)
            {
                Debug.Log($"Equippable slots {_equippedSpells.Length} < total slots {gameSettings.TotalSpellSlots}, resetting equippable.");
                _equippedSpells = new SpellData[gameSettings.TotalSpellSlots];
            }
            return _equippedSpells;
        }
        set
        {
            Debug.Log($"Setting EquippedSpells to {value}");
            _equippedSpells = value;
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
    private CharacterStats _characterStats;
    public CharacterStats CharacterStats
    {
        get
        {
            if (CharacterObject == null)
            {
                Debug.LogWarning("CharacterObject null, setting CharacterStats to null");
                _characterStats = null;
            }
            else if (_characterStats == null)
            {
                _characterStats = CharacterObject.GetComponent<CharacterStats>();
            }
            return _characterStats;
        }
    }
    // Spell manager object
    private SpellManager _characterSpellManagerObject;
    public SpellManager CharacterSpellManager
    {
        get
        {
            if (_characterSpellManagerObject == null)
            {
                GameObject spellManagerObject = TaggedObjectWithType<SpellManager>();
                if (spellManagerObject != null)
                {
                    _characterSpellManagerObject = spellManagerObject.GetComponent<SpellManager>();
                }
                else
                {
                    _characterSpellManagerObject = null;
                }
            }
            return _characterSpellManagerObject;
        }
    }
    // Spellbook script
    private SpellbookLogic _spellbookLogicScript;
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
