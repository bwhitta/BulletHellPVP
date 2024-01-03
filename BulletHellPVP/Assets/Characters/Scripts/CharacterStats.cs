using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using static BarLogic;

public class CharacterStats : MonoBehaviour
{
    [HideInInspector] public CharacterInfo characterInfo;

    //private float remainingInvincibilityTime = 0;

    // Mana scaling
    private float manaScalingTime;
    [HideInInspector] public float maxMana;

    private readonly List<float> effectManaRegenTimer = new();
    private readonly List<float> effectManaRegenValues = new();

    // Health
    private float? _currentHealthStat;
    public float CurrentHealthStat
    {
        get
        {
            _currentHealthStat ??= GameSettings.Used.MaxHealth;
            return (float)_currentHealthStat;
        }
        set
        {
            _currentHealthStat ??= GameSettings.Used.MaxHealth;

            /*// Mana Damage Ratio (UNUSED)
            if (value < _currentHealthStat)
            {
                float damageTaken = (float)_currentHealthStat - value;
                float amountToRegen = damageTaken * characterInfo.DefaultStats.RegenOnDamageMod;
                float modifiedByTime = amountToRegen / characterInfo.DefaultStats.ManaDamageRegenTime;

                effectManaRegenTimer.Add(characterInfo.DefaultStats.ManaDamageRegenTime);
                effectManaRegenValues.Add(modifiedByTime);
                Debug.Log($"Damage taken: {damageTaken}. " +
                    $"Amount to regen: {amountToRegen}. " +
                    $"Per second: {modifiedByTime}. ");
            }*/

            if (value > GameSettings.Used.MaxHealth)
            {
                _currentHealthStat = GameSettings.Used.MaxHealth;
            }
            else
            {
                _currentHealthStat = value;
            }

            characterInfo.HealthBar.UpdateStatDisplays(UpdatableStats.Remaining);

            if (value <= 0)
            {
                _currentHealthStat = 0;
                Debug.Log("dead");
                Destroy(gameObject);
            }
        }
    }

    // Mana
    private float? _currentManaStat;
    public float CurrentManaStat
    {
        get
        {
            _currentManaStat ??= maxMana;
            return (float)_currentManaStat;
        }
        set
        {
            _currentManaStat ??= maxMana;
            if (value > maxMana)
                _currentManaStat = maxMana;
            else
                _currentManaStat = value;

            characterInfo.ManaBar.UpdateStatDisplays(UpdatableStats.Remaining);
        }
    }

    #region Monobehavior Methods
    private void OnEnable()
    {
        CharacterEnabled(true);
    }
    private void OnDisable()
    {
        CharacterEnabled(false);
    }
    private void Update()
    {
        //InvincibilityTick();
        ManaScalingTick();
        ManaRegenTick();
    }
    #endregion

    // Actions upon enabling or disabling character
    private void CharacterEnabled(bool enable)
    {
        if (enable)
        {
            // Get character info
            characterInfo = CharacterInfoManager.JoinAvailableLocation();
            if (characterInfo == null)
            {
                Debug.Log("Character destroyed - no available slot found");
                Destroy(gameObject);
                return;
            }
            transform.position = characterInfo.CharacterStartLocation;
            name = characterInfo.name;

            // Mana
            maxMana = GameSettings.Used.StartingMaxMana;
        }
        else
        {
            if (characterInfo == null || characterInfo.SpellbookLogicScript == null)
            {
                Debug.Log("Skipping disable, some info was found null");
                return;
            }
        }

        // Check tag
        if (gameObject.CompareTag("Untagged"))
        {
            gameObject.tag = characterInfo.CharacterAndSortingTag;
        }

        // Enable other objects
        characterInfo.HealthBar.BarEnabled(enable);
        characterInfo.ManaBar.BarEnabled(enable);
        characterInfo.SpellbookLogicScript.SpellbookToggle(enable);
    }

    
    private void ManaScalingTick()
    {
        if (manaScalingTime < GameSettings.Used.ManaScalingTime)
        {
            manaScalingTime += Time.deltaTime;
            float percentageCompleted = manaScalingTime / GameSettings.Used.ManaScalingTime;
            maxMana = Calculations.RelativeTo(GameSettings.Used.StartingMaxMana, GameSettings.Used.EndingMaxMana, percentageCompleted);
        }
    }
    private void ManaRegenTick()
    {
        float scalingPercent = manaScalingTime / GameSettings.Used.ManaScalingTime;
        float deltaManaChange = Calculations.RelativeTo(GameSettings.Used.StartingManaRegen, GameSettings.Used.EndingManaRegen, scalingPercent);

        // Temporary mana regen from effects
        for (int i = 0; i < effectManaRegenValues.Count; i++)
        {
            deltaManaChange += effectManaRegenValues[i];

            // Update timer
            effectManaRegenTimer[i] -= Time.deltaTime;
            if (effectManaRegenTimer[i] <= 0)
            {
                effectManaRegenTimer.RemoveAt(i);
                effectManaRegenValues.RemoveAt(i);
            }
        }
        CurrentManaStat += deltaManaChange * Time.deltaTime;
    }

    /*
    private void InvincibilityTick()
    {
        if (remainingInvincibilityTime > 0)
        {
            remainingInvincibilityTime -= Time.deltaTime;

            // Check if completed
            if (remainingInvincibilityTime <= 0)
            {
                remainingInvincibilityTime = 0;
                SetChildAlpha(1);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckCollision(collision);
    }
    private void CheckCollision(Collider2D collision)
    {
        if (gameObject.activeSelf == false)
        {
            return;
        }

        if (collision.GetComponent<SpellModuleBehavior>() != null)
        {
            SpellModuleBehavior collisionSpellBehavior = collision.GetComponent<SpellModuleBehavior>();

            Debug.Log($"Some collision code is currently disabled.");
            if(remainingInvincibilityTime <= 0 && collisionSpellBehavior.module.AbilityDealsDamage)
                DamageDealt(collisionSpellBehavior);
        }
        
        void DamageDealt(SpellModuleBehavior collisionSpellBehavior)
        {
            remainingInvincibilityTime = characterInfo.DefaultStats.InvincibilityTime;
            SetChildAlpha(characterInfo.DefaultStats.InvincibilityAlphaMod);

            gameObject.GetComponent<CharacterStats>().CurrentHealthStat -= collisionSpellBehavior.module.Damage;
            float percentageCompleted = manaScalingTime / characterInfo.DefaultStats.ScalingTime;
            float manaRegen = Calculations.RelativeTo(characterInfo.DefaultStats.StartingManaRegen, characterInfo.DefaultStats.EndingManaRegen, percentageCompleted);
            CurrentManaStat += manaRegen * Time.deltaTime;
        }
    }
    private void SetChildAlpha(float alpha)
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            gameObject.transform.GetChild(i).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha);
        }
    }
    */
}
