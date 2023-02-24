using UnityEngine;
using UnityEngine.InputSystem;
using static ControlsManager;

public class ControlCharacter : MonoBehaviour
{
        [Header("Controls and movement")]
    public float MovementSpeedMod; // Movement speed multiplier
    [SerializeField] private Animator CharacterAnimator; // The animator object with the animation tree use
    [SerializeField] private string AnimatorTreeParameterX, AnimatorTreeParameterY; // The names of the parameters for the animation tree
    private GameControls controls;
    private InputAction movement;
    private Rigidbody2D playerRigidbody;

        [Header("Player Stats")]
    [SerializeField] private float maxHealth;
    [SerializeField] private float maxMana;
    [HideInInspector] public PlayerStats stats;
    [SerializeField] private GameObject healthBarObject, manaBarObject;
    private ConsumableBarLogic healthBar, manaBar;

    private void Awake() // called before all starts
    {
        PlayerStatSetup(maxHealth, maxMana);
        healthBar = healthBarObject.GetComponent<ConsumableBarLogic>();
        manaBar = manaBarObject.GetComponent<ConsumableBarLogic>();

        playerRigidbody = gameObject.GetComponent<Rigidbody2D>();

    }
    private void Start()
    {
        controls = GameControlsMaster.GameControls;

        movement = controls.Player.Movement;
        movement.Enable();
    }
    
    private void Update()
    {
        Vector2 movementVector = movement.ReadValue<Vector2>(); // Reads the "movement" input action's vector
        

        playerRigidbody.velocity = MovementSpeedMod * movementVector; // Moves the character

        CharacterAnimator.SetFloat("FacingX", movementVector.x); // Tells the animator to show the character as facing left or right
        CharacterAnimator.SetFloat("FacingY", movementVector.y); // Tells the animator to show the character as facing up or down
    }

    private void PlayerStatSetup(float maximumHealth, float maximumMana)
    {
        //Creates and sets new stats unique to this character
        stats = new PlayerStats
        {
            MaxHealthStat = maximumHealth,
            CurrentHealthStat = maximumHealth,
            MaxManaStat = maximumMana,
            CurrentManaStat = maximumMana
        };
    }
    public enum Stat { Health, Mana }
    public void ModifyStat(Stat statToModify, float amount)
    {
        if (statToModify == Stat.Health)
        {
            healthBar.ModifyStat(amount);
        }
        else if (statToModify == Stat.Mana)
        {
            manaBar.ModifyStat(amount);
        }
        else
        {
            Debug.LogError("Inputted stat enum invalid");
        }

    }
    public class PlayerStats
    {
        // Max Health and Mana
        public float MaxManaStat;
        public float MaxHealthStat;

        // Current Health
        private float _currentHealthStat;
        public float CurrentHealthStat
        {
            get => _currentHealthStat;
            set
            {
                if(_currentHealthStat > MaxHealthStat)
                    _currentHealthStat = MaxHealthStat;

                else
                    _currentHealthStat = value;
            }
        }

        // Current Mana
        private float _currentManaStat;
        public float CurrentManaStat
        {
            get => _currentManaStat;
            set
            {
                if (_currentManaStat > MaxManaStat)
                    _currentManaStat = MaxManaStat;

                else
                    _currentManaStat = value;
            }
        }
    }
}
