using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Player")]
public class PlayerInfo : ScriptableObject
{
    public BasicStats defaultStats;
    [Space]
    public string PlayerTag;
    public Vector2 PlayerStart;
    public string ControllingMap;
    public string MovementActionName;
    [Space]
    public Animator CharacterAnimator;
    public string AnimatorTreeParameterX, AnimatorTreeParameterY;

    // Health
    private BarLogic _healthBar;
    public BarLogic HealthBar
    {
        get
        {
            if (_healthBar == null)
                _healthBar = FindBar(BarLogic.Stats.health);
            return _healthBar;
        }
    }

    // Mana
    private BarLogic _manaBar;
    public BarLogic ManaBar
    {
        get
        {
            if(_manaBar==null)
                _manaBar = FindBar(BarLogic.Stats.mana);
            return _manaBar;
        }
    }

    private BarLogic FindBar(BarLogic.Stats barStat)
    {
        // Find the list of objects that share a tag with the player
        GameObject[] tagged = GameObject.FindGameObjectsWithTag(PlayerTag);

        // Find the health bar in the list
        for (int i = 0; i < tagged.Length; i++)
        {
            if (tagged[i].GetComponent<BarLogic>().statToModify == barStat)
            {
                return tagged[i].GetComponent<BarLogic>();
            }
        }
        Debug.LogError("No tagged bar found");
        return null;
    }

    // PlayerStats object
    private CharacterStats _player;
    public CharacterStats PlayerStats
    {
        get
        {
            if (_player == null)
                _player = FindPlayer();
            return _player;
        }
    }
    
    private CharacterStats FindPlayer()
    {
        // Find the list of objects that share a tag with the player
        GameObject[] tagged = GameObject.FindGameObjectsWithTag(PlayerTag);

        // Find the health bar in the list
        for (int i = 0; i < tagged.Length; i++)
        {
            if (tagged[i].GetComponent<CharacterStats>() != null)
                return tagged[i].GetComponent<CharacterStats>();
        }
        Debug.LogError("No tagged player found");
        return null;
    }
}
