using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static BarLogic;

public class CharacterStats : MonoBehaviour
{
    
    public PlayerInfo playerInfo;

    private float remainingInvincibilityTime = 0;
    
    private void Update()
    {
        InvincibilityTick();
    }

    // Health
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
            if (value > playerInfo.defaultStats.MaxHealthStat)
                _currentHealthStat = playerInfo.defaultStats.MaxHealthStat;

            else
                _currentHealthStat = value;
            playerInfo.HealthBar.UpdateStatDisplay(UpdatableStats.Remaining);

            if (value < 0)
            {
                _currentHealthStat = 0;
                Debug.Log("dead");
                GameplayManager.GameIsOver = true;
                Destroy(gameObject);
            }
        }
    }

    // Mana
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
            if (value > playerInfo.defaultStats.MaxManaStat)
                _currentManaStat = playerInfo.defaultStats.MaxManaStat;
            else
                _currentManaStat = value;
            playerInfo.ManaBar.UpdateStatDisplay(UpdatableStats.Remaining);
        }
    }

    // Check if the stats are configured
    private bool statsConfigured;
    private void StatConfigCheck()
    {
        if (!statsConfigured)
        {
            _currentHealthStat = playerInfo.defaultStats.MaxHealthStat;
            _currentManaStat = playerInfo.defaultStats.MaxManaStat;
        }
        statsConfigured = true;
    }

    // Collision
    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckCollision(collision);
    }
    private void CheckCollision(Collider2D collision)
    {
        Debug.Log($"{gameObject.name} is colliding with {collision.gameObject.name}");
        if (GameplayManager.GameIsOver)
        {
            return;
        }

        if (collision.GetComponent<SpellBehavior>() != null)
        {
            SpellBehavior collisionSpellBehavior = collision.GetComponent<SpellBehavior>();
            
            if(remainingInvincibilityTime <= 0)
            {
                remainingInvincibilityTime = playerInfo.defaultStats.InvincibilityTime;
                gameObject.GetComponent<CharacterStats>().CurrentHealthStat -= collisionSpellBehavior.spellData.Damage;
                Debug.Log($"{collisionSpellBehavior.spellData.Damage} health lost ");
            }
        }
    }
    
    // Invincibility after damage
    private void InvincibilityTick()
    {
        if (remainingInvincibilityTime > 0)
        {
            remainingInvincibilityTime -= Time.deltaTime;

            SetChildAlpha(playerInfo.defaultStats.InvincibilityAlphaMod);
        }
        if (remainingInvincibilityTime < 0)
        {
            remainingInvincibilityTime = 0;
            SetChildAlpha(1);
        }

        CurrentManaStat += playerInfo.defaultStats.BaseManaRegen * Time.deltaTime;
    }
    private float currentAlpha;
    private void SetChildAlpha(float alpha)
    {
        if(Mathf.Approximately(currentAlpha, alpha))
        {
            return;
        }
        currentAlpha = alpha;
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            gameObject.transform.GetChild(i).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha);
        }
    }
}
