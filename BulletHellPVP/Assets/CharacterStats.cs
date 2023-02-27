using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConsumableBarLogic;

public class CharacterStats : MonoBehaviour
{
    [SerializeField] private GameObject healthBarObject, manaBarObject;
    public float MaxManaStat, MaxHealthStat;
    private ConsumableBarLogic healthBar, manaBar;

    // Check if the stats are configured
    private bool statsConfigured;
    private void StatConfigCheck()
    {
        if (!statsConfigured)
        {
            healthBar = healthBarObject.GetComponent<ConsumableBarLogic>();
            manaBar = manaBarObject.GetComponent<ConsumableBarLogic>();
            _currentHealthStat = MaxHealthStat;
            _currentManaStat = MaxManaStat;
        }
        statsConfigured = true;
    }

    // Current Health
    private float _currentHealthStat;
    public float CurrentHealthStat
    {
        get
        {
            StatConfigCheck();
            return _currentHealthStat;
        }
        set
        {
            StatConfigCheck();
            if (_currentHealthStat > MaxHealthStat)
                _currentHealthStat = MaxHealthStat;

            else
                _currentHealthStat = value;
            healthBar.UpdateStatDisplay(UpdatableStats.Remaining);
        }
    }

    // Current Mana
    private float _currentManaStat;
    public float CurrentManaStat
    {
        get
        {
            StatConfigCheck();
            return _currentManaStat;
        }
        set
        {
            StatConfigCheck();
            if (_currentManaStat > MaxManaStat)
                _currentManaStat = MaxManaStat;

            else
                _currentManaStat = value;
            manaBar.UpdateStatDisplay(UpdatableStats.Remaining);
        }
    }
}
