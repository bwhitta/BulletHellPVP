using Unity.Netcode;
using UnityEngine;

public class CharacterStats : NetworkBehaviour
{
    private float remainingInvincibilityTime = 0;
    [SerializeField] private BarLogic healthBar;
    [SerializeField] private BarLogic manaBar;
    [SerializeField] private CharacterManager characterManager;

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
    private readonly NetworkVariable<float> ServerSideHealth = new();

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

            manaBar.StatValue = _currentMana;
        }
    }
    private readonly NetworkVariable<float> ServerSideMana = new();
    [HideInInspector] public int ManaAwaitingCountdown = 0;
    [HideInInspector] public float ManaAwaiting;
    private float manaScalingTime;
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

    // Network
    private byte ticksSinceUpdate;
    
    // Methods
    public void Start()
    {
        healthBar.StatMax = GameSettings.Used.MaxHealth;
        MaxMana = GameSettings.Used.StartingMaxMana;
        manaBar.StatMax = GameSettings.Used.StartingMaxMana;

        CurrentHealth = GameSettings.Used.MaxHealth;
        CurrentMana = GameSettings.Used.StartingMaxMana;
        
        if (MultiplayerManager.IsOnline)
        {
            if (!IsServer)
            {
                ServerSideHealth.OnValueChanged += ServerHealthUpdate;
                ServerSideMana.OnValueChanged += ServerManaUpdate;
            }
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
    private void FixedUpdate()
    {
        if (!MultiplayerManager.GameStarted) return;

        if (remainingInvincibilityTime > 0) InvincibilityTick();
        ManaScalingTick();
        ManaRegenTick();
        ManaAwaitingTick();
        if (IsServer) ServerDiscrepancyTick();

        void ServerDiscrepancyTick()
        {
            // Discrepancy check ticks
            ticksSinceUpdate++;
            if (ticksSinceUpdate >= GameSettings.Used.NetworkDiscrepancyCheckFrequency)
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
                    Debug.Log("Countdown complete!");
                }
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (gameObject.activeSelf == false)
        {
            Debug.Log($"Self is not active"); // temp log
            return;
        }

        if (collision.GetComponent<SpellModuleBehavior>() != null)
        {
            SpellModuleBehavior collisionSpellBehavior = collision.GetComponent<SpellModuleBehavior>();
            if (remainingInvincibilityTime <= 0 && collisionSpellBehavior.Module.AbilityDealsDamage)
            {
                DamageDealt(collisionSpellBehavior);
            }
        }

        // Local Methods
        void DamageDealt(SpellModuleBehavior collisionSpellBehavior)
        {
            remainingInvincibilityTime = GameSettings.Used.InvincibilityTime;
            SetChildAlpha(GameSettings.Used.InvincibilityAlphaMod);

            GetComponent<CharacterStats>().CurrentHealth -= collisionSpellBehavior.Module.Damage;
            Debug.Log($"Damage dealt - total of {collisionSpellBehavior.Module.Damage} health lost.");
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
}
