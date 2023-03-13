using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Player")]
public class PlayerInfo : ScriptableObject
{
    public BasicStats defaultStats;
    [Space(25)]
    public PlayerInfo opponentPlayerInfo;
    [Space(25)]
    public string CharacterTag;
    public Vector2 CharacterStartLocation;
    [Space(25)]
    public string InputMapName;
    public string MovementActionName;
    public string SpellbookSelectionActionName;
    [Space(25)]
    public string AnimatorTreeParameterX;
    public string AnimatorTreeParameterY;

    
    /*public bool CheckObjectsExist()
    {
        if (HealthBar == null || ManaBar == null || SpellbookLogicScript == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }*/

    // Health and Mana
    private BarLogic _healthBar;
    private BarLogic _manaBar;
    public BarLogic HealthBar
    {
        get
        {
            if (_healthBar == null)
                _healthBar = FindBar(BarLogic.Stats.health);
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
                _manaBar = FindBar(BarLogic.Stats.mana);
            return _manaBar;
        }
        set
        {
            _manaBar = value;
        }
    }
    
    private BarLogic FindBar(BarLogic.Stats barStat)
    {
        // Find the list of objects that share a tag with the player
        GameObject[] tagged = GameObject.FindGameObjectsWithTag(CharacterTag);

        // Find the health bar in the list
        for (int i = 0; i < tagged.Length; i++)
        {
            if (tagged[i].GetComponent<BarLogic>() != null && tagged[i].GetComponent<BarLogic>().statToModify == barStat)
            {
                return tagged[i].GetComponent<BarLogic>();
            }
        }
        Debug.LogWarning("No tagged bar found");
        return null;
    }

    // PlayerObject
    private GameObject _playerObject;
    public GameObject PlayerObject // should be named PlayerObject
    {
        get
        {
            if (_playerObject == null)
            {
                _playerObject = TaggedObjectWithType<CharacterStats>(CharacterTag);
            }
            return _playerObject;
        }
    }

    // CharacterStatsScript object
    private CharacterStats _characterStatScript;
    public CharacterStats CharacterStatsScript
    {
        get
        {
            if (PlayerObject == null)
            {
                Debug.LogWarning("PlayerObject null, setting PlayerStats to null");
                _characterStatScript = null;
            }
            else if (_characterStatScript == null)
            {
                _characterStatScript = PlayerObject.GetComponent<CharacterStats>();
            }
            return _characterStatScript;
        }
    }
    
    // Spellcasting objects
    private SpellManager _characterSpellManagerObject;
    public SpellManager CharacterSpellManager
    {
        get
        {
            if (_characterSpellManagerObject == null)
            {
                GameObject spellManagerObject = TaggedObjectWithType<SpellManager>(CharacterTag);
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

    // Spellbook
    private SpellbookLogic _spellbookLogicScript;
    public SpellbookLogic SpellbookLogicScript
    {
        get
        {
            if(_spellbookLogicScript == null)
            {
                GameObject spellbookObject = TaggedObjectWithType<SpellbookLogic>(CharacterTag);
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

    // Finding objects
    private GameObject TaggedObjectWithType<objectType>(string tag)
    {
        // Find the list of objects that share a tag with the player
        GameObject[] tagged = GameObject.FindGameObjectsWithTag(tag);

        // Find the health bar in the list
        for (int i = 0; i < tagged.Length; i++)
        {
            // Checks if tagged[i] has the right objectType attatched
            if (tagged[i].GetComponent(typeof(objectType)))
            {
                return tagged[i];
            }
        }
        return null;
    }

}
