using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterStats : NetworkBehaviour
{
    [HideInInspector] public CharacterInfo characterInfo;

    private float remainingInvincibilityTime = 0;

    // Mana scaling
    private float manaScalingTime;
    [HideInInspector] public float maxMana;

    private readonly List<float> effectManaRegenTimer = new();
    private readonly List<float> effectManaRegenValues = new();

    // Health
    private float? _currentHealth;
    public float CurrentHealth
    {
        get
        {
            _currentHealth ??= GameSettings.Used.MaxHealth;
            return (float)_currentHealth;
        }
        set
        {
            _currentHealth ??= GameSettings.Used.MaxHealth;

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
                _currentHealth = GameSettings.Used.MaxHealth;
            }
            else
            {
                _currentHealth = value;
            }

            characterInfo.HealthBar.UpdateStatDisplays(BarLogic.UpdatableStats.Remaining);

            if (value <= 0)
            {
                _currentHealth = 0;
                Debug.Log("dead");
                Destroy(gameObject);
            }
        }
    }
    private readonly NetworkVariable<float> ServerSideHealth = new();

    // Mana
    private float? _currentMana;
    public float CurrentMana
    {
        get
        {
            _currentMana ??= maxMana;
            return (float)_currentMana;
        }
        set
        {
            _currentMana ??= maxMana;
            if (value > maxMana)
                _currentMana = maxMana;
            else
                _currentMana = value;

            characterInfo.ManaBar.UpdateStatDisplays(BarLogic.UpdatableStats.Remaining);
        }
    }
    private readonly NetworkVariable<float> ServerSideMana = new();
    
    public int ManaAwaitingCountdown = 0;
    public float ManaAwaiting;

    // Network
    private byte ticksSinceUpdate;
    
    private void FixedUpdate()
    {
        if (remainingInvincibilityTime > 0) InvincibilityTick();
        ManaScalingTick();
        ManaRegenTick();
        ManaAwaitingTick();
        if (IsServer) ServerTick();
        
        void ServerTick()
        {
            ticksSinceUpdate++;
            if(ticksSinceUpdate >= GameSettings.Used.NetworkDiscrepancyCheckFrequency)
            {
                ServerSideHealth.Value = CurrentHealth;
                ServerSideMana.Value = CurrentMana;
                ticksSinceUpdate = 0;
            }
        }
        void ManaAwaitingTick()
        {
            if (ManaAwaiting <= 0)
            {
                ManaAwaiting = 0;
                ManaAwaitingCountdown = 0;
            }
            if (ManaAwaitingCountdown > 0)
            {
                ManaAwaitingCountdown--;
                if (ManaAwaitingCountdown <= 0)
                {
                    ManaAwaiting = 0;
                    Debug.LogWarning("Countdown complete!");
                }
            }
        }
    }

    // Actions upon enabling or disabling character
    private void OnEnable()
    {
        CharacterEnabled(true);
        if (MultiplayerManager.IsOnline && !IsServer) NetworkVariableListeners();
        
        void NetworkVariableListeners()
        {
            ServerSideHealth.OnValueChanged += ServerHealthUpdate;
            ServerSideMana.OnValueChanged += ServerManaUpdate;
        }
        void ServerHealthUpdate(float oldValue, float newValue)
        {
            CurrentHealth = Calculations.DiscrepancyCheck(CurrentHealth, newValue, GameSettings.Used.NetworkStatBarDiscrepancyLimit);
        }
        void ServerManaUpdate(float oldValue, float newValue)
        {
            // If a spell has mana deducted on client-side but has yet to reach the server, the client-side mana should treat the server-side mana as though it were that much lower.
            // Otherwise, the mana will rubber band up on one server mana tick then rubber band back down the next.
            float adjustedServerMana = newValue - ManaAwaiting;

            if (Mathf.Abs(CurrentMana - adjustedServerMana) > GameSettings.Used.NetworkStatBarDiscrepancyLimit)
            {
                Debug.LogWarning($"Discepancy: setting CurrentMana to {newValue}, CurrentMana: {CurrentMana}, ManaAwaiting: {ManaAwaiting}");
                CurrentMana = adjustedServerMana;
            }
        }
    }
    private void OnDisable()
    {
        CharacterEnabled(false);
    }
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
            name = characterInfo.name;

            // Starting position
            transform.position = characterInfo.CharacterStartLocation;
            
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

    // Mana scaling over time
    private void ManaScalingTick()
    {        
        // Skip scaling if opponent is not connected
        if (characterInfo.OpponentCharacterInfo.CharacterObject == null) return;

        // Mana scaling end
        if (manaScalingTime > GameSettings.Used.ManaScalingTime) return;

        manaScalingTime += Time.fixedDeltaTime;
        float percentageCompleted = manaScalingTime / GameSettings.Used.ManaScalingTime;
        maxMana = Calculations.RelativeTo(GameSettings.Used.StartingMaxMana, GameSettings.Used.EndingMaxMana, percentageCompleted);
        
    }

    // Mana regenerating over time
    private void ManaRegenTick()
    {
        float scalingPercent = manaScalingTime / GameSettings.Used.ManaScalingTime;
        float deltaManaChange = Calculations.RelativeTo(GameSettings.Used.StartingManaRegen, GameSettings.Used.EndingManaRegen, scalingPercent);

        // Temporary mana regen from effects
        for (int i = 0; i < effectManaRegenValues.Count; i++)
        {
            deltaManaChange += effectManaRegenValues[i];

            // Update timer
            effectManaRegenTimer[i] -= Time.fixedDeltaTime;
            if (effectManaRegenTimer[i] <= 0)
            {
                effectManaRegenTimer.RemoveAt(i);
                effectManaRegenValues.RemoveAt(i);
            }
        }
        CurrentMana += deltaManaChange * Time.fixedDeltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {   
        Debug.Log($"Trigger entered! Collision2D: {collision}"); // temp log
        CheckCollision(collision);
    }

    private void CheckCollision(Collider2D collision)
    {
        if (gameObject.activeSelf == false)
        {
            Debug.Log($"Self is not active"); // temp log
            return;
        }

        if (collision.GetComponent<SpellModuleBehavior>() != null)
        {
            SpellModuleBehavior collisionSpellBehavior = collision.GetComponent<SpellModuleBehavior>();
            Debug.Log($"Spell module is not null: {collision.GetComponent<SpellModuleBehavior>()}");
            if (remainingInvincibilityTime <= 0 && collisionSpellBehavior.Module.AbilityDealsDamage)
            {
                DamageDealt(collisionSpellBehavior);
            }
        }
        
        
        void DamageDealt(SpellModuleBehavior collisionSpellBehavior)
        {
            remainingInvincibilityTime = GameSettings.Used.InvincibilityTime;
            SetChildAlpha(GameSettings.Used.InvincibilityAlphaMod);

            gameObject.GetComponent<CharacterStats>().CurrentHealth -= collisionSpellBehavior.Module.Damage;
            Debug.Log($"Damage dealt - total of {collisionSpellBehavior.Module.Damage} health lost.");
        }
    }

    private void InvincibilityTick()
    {
        remainingInvincibilityTime -= Time.fixedDeltaTime;

        // Check if completed
        if (remainingInvincibilityTime <= 0)
        {
            remainingInvincibilityTime = 0;
            SetChildAlpha(1);
        }
        
    }
    
    private void SetChildAlpha(float alpha)
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            gameObject.transform.GetChild(i).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha);
        }
    }
}
