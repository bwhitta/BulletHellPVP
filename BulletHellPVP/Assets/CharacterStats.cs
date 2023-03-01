using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConsumableBarLogic;

public class CharacterStats : MonoBehaviour
{
        [Header("Mana and health")]
    [SerializeField] private GameObject healthBarObject, manaBarObject;
    public float MaxManaStat, MaxHealthStat;
    private ConsumableBarLogic healthBar, manaBar;
    
        [Header("Damage")]
    [SerializeField] private float maxInvincibilityTime;
    [SerializeField][Range(0, 1)] private float invincibilityAlphaMod;
    private float remainingInvincibilityTime = 0;

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
    // Collision
    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckCollision(collision);
    }

    private void CheckCollision(Collider2D collision)
    {
        Debug.Log($"{gameObject.name} is colliding with {collision.gameObject.name}");

        if (collision.GetComponent<SpellBehavior>() != null)
        {
            SpellBehavior collisionSpellBehavior = collision.GetComponent<SpellBehavior>();
            
            if(remainingInvincibilityTime <= 0)
            {
                remainingInvincibilityTime = maxInvincibilityTime;
                gameObject.GetComponent<CharacterStats>().CurrentHealthStat -= collisionSpellBehavior.spellData.Damage;
                Debug.Log($"{collisionSpellBehavior.spellData.Damage} health lost ");
            }
        }
    }

    private void Update()
    {
        if(remainingInvincibilityTime > 0)
        {
            remainingInvincibilityTime -= Time.deltaTime;
            
            SetChildAlpha(invincibilityAlphaMod);
        }
        if(remainingInvincibilityTime < 0)
        {
            remainingInvincibilityTime = 0;
            SetChildAlpha(1);
        }
    }

    private float currentAlpha;
    private void SetChildAlpha(float alpha)
    {
        if(Mathf.Approximately(currentAlpha, alpha))
        {
            return;
        }
        currentAlpha = alpha;
        Debug.Log("Setting alpha");
        for(var i = 0; i < gameObject.transform.childCount; i++)
        {
            Debug.Log(i);
            gameObject.transform.GetChild(i).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha);
        }
    }
}
