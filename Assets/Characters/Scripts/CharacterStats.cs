using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

public class CharacterStats : NetworkBehaviour
{
    // Fields
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private BarLogic healthBar;
    [SerializeField] private BarLogic manaBar;
    private float remainingInvincibilityTime = 0;

    // Health
    private float _currentHealth;
    private float CurrentHealth
    {
        get => _currentHealth;
        set
        {

            if (value > GameSettings.Used.MaxHealth)
            {
                _currentHealth = GameSettings.Used.MaxHealth;
            }
            else if (value <= 0)
            {
                _currentHealth = 0;
                Debug.Log("dead");
                Destroy(gameObject);
            }
            else
            {
                _currentHealth = value;
            }

            healthBar.StatValue = _currentHealth;
        }
    }

    // Mana
    private float _currentMana;
    public float CurrentMana
    {
        get => _currentMana;
        set
        {
            if (value > MaxMana)
                _currentMana = MaxMana;
            else
                _currentMana = value;

            // probably better eventually to make a ManaChanged event, and then the stat bars are updated based on that event. do the same for health too.
            manaBar.StatValue = _currentMana;
        }
    }
    private float NonAwaitingMana
    {
        get
        {
            return CurrentMana - ManaAwaiting;
        }
    }

    private float _maxMana;
    public float MaxMana
    {
        get => _maxMana;
        set
        {
            _maxMana = value;
            manaBar.StatMax = _maxMana;
        }
    }

    [HideInInspector] public float ManaAwaiting;
    
    private float manaScalingTime;

    // Online
    private byte ticksSinceUpdate;
    
    // Methods
    private void Start()
    {
        // Set up stat bar maximums
        healthBar.StatMax = GameSettings.Used.MaxHealth;
        manaBar.StatMax = GameSettings.Used.StartingMaxMana;
        
        // Set up health and mana
        MaxMana = GameSettings.Used.StartingMaxMana;
        CurrentHealth = GameSettings.Used.MaxHealth;
        CurrentMana = GameSettings.Used.StartingMaxMana;
    }
    private void FixedUpdate()
    {
        if (!MultiplayerManager.GameStarted) return;

        if (remainingInvincibilityTime > 0) InvincibilityTick();
        ManaScalingTick();
        ManaRegenTick();
        //ManaAwaitingTick();
        if (IsServer) ServerDiscrepancyTick();

        void ServerDiscrepancyTick()
        {
            // Discrepancy check ticks
            ticksSinceUpdate++;
            if (ticksSinceUpdate >= GameSettings.Used.NetworkDiscrepancyCheckFrequency)
            {
                ServerHealthUpdateRpc(CurrentHealth);
                ServerManaUpdateRpc(CurrentMana);
                ticksSinceUpdate = 0;
            }
        }
        /*void ManaAwaitingTick()
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
                    Debug.Log("Countdown complete!");
                }
            }
        }*/
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (gameObject.activeSelf == false)
        {
            Debug.Log($"gameObject is not active"); // temp log
            return;
        }
        if (collision.TryGetComponent<Spell>(out var spell))
        {
            if (remainingInvincibilityTime <= 0 && spell.Module.DealsDamage && spell.TargetId == characterManager.CharacterIndex)
            {
                DamageDealt(spell);
            }
        }

        // Local Methods
        void DamageDealt(Spell spell)
        {
            remainingInvincibilityTime = GameSettings.Used.InvincibilityTime;
            SetChildAlpha(GameSettings.Used.InvincibilityAlphaMod);

            CurrentHealth -= spell.Module.Damage;
            Debug.Log($"Damage dealt - total of {spell.Module.Damage} health lost.");
        }
    }
    private void ManaScalingTick()
    {
        // Don't start scaling mana until both players have joined
        if (!MultiplayerManager.GameStarted) return;

        // Mana scaling end
        if (manaScalingTime > GameSettings.Used.ManaScalingTime) return;

        manaScalingTime += Time.fixedDeltaTime;
        float percentageCompleted = manaScalingTime / GameSettings.Used.ManaScalingTime;
        MaxMana = Calculations.RelativeTo(GameSettings.Used.StartingMaxMana, GameSettings.Used.EndingMaxMana, percentageCompleted);
    }
    private void ManaRegenTick()
    {
        float scalingPercent = manaScalingTime / GameSettings.Used.ManaScalingTime;
        float manaChangeRate = Calculations.RelativeTo(GameSettings.Used.StartingManaRegen, GameSettings.Used.EndingManaRegen, scalingPercent);

        CurrentMana += manaChangeRate * Time.fixedDeltaTime;
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
        foreach (Transform child in transform)
        {
            child.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha);
        }
    }

    // Networking
    [Rpc(SendTo.NotServer)]
    private void ServerHealthUpdateRpc(float newValue)
    {
        CurrentHealth = Calculations.DiscrepancyCheck(CurrentHealth, newValue, GameSettings.Used.NetworkStatBarDiscrepancyLimit);
    }
    [Rpc(SendTo.NotServer)]
    private void ServerManaUpdateRpc(float newValue)
    {
        CurrentMana = Calculations.DiscrepancyCheck(NonAwaitingMana, newValue, GameSettings.Used.NetworkStatBarDiscrepancyLimit);
    }
}
