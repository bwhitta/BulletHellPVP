using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

public class CharacterStats : NetworkBehaviour
{
    // Fields
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private BarLogic healthBar;
    [SerializeField] private BarLogic manaBar;
    public delegate void OnStatChanged(float change);

    // Health
    // could make this into a custom type called Stat, which has a current value and max value, and automatically calls an event when it changes.
    public OnStatChanged HealthChanged;
    private float _currentHealth;
    public float CurrentHealth
    {
        get => _currentHealth;
        set
        {
            // Trigger event
            HealthChanged?.Invoke(value - _currentHealth);

            // Limit current health to max health
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

            // could make updating the healthbar subscribed to the OnHealthChanged event as well
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
        
        // Set starting values
        MaxMana = GameSettings.Used.StartingMaxMana;
        CurrentHealth = GameSettings.Used.MaxHealth;
        CurrentMana = GameSettings.Used.StartingMaxMana;
    }
    private void FixedUpdate()
    {
        if (!MultiplayerManager.GameStarted) return;

        ManaScalingTick();
        ManaRegenTick();
        //ManaAwaitingTick();
        if (IsServer) ServerDiscrepancyTick();

        
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

    // Networking
    private void ServerDiscrepancyTick()
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
