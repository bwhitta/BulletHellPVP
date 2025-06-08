using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Game Settings")]
public class GameSettings : ScriptableObject
{
    public static GameSettings Used;

    [Header("Spells")]
    public byte TotalBooks;
    public bool CanLoopBooks;
    public byte SpellSlots;
    public SpellSet[] SpellSets;

    [Header("Play Area")]
    public float BattleSquareWidth;
    public Vector2[] BattleAreaCenters;
    public string[] spellMaskLayers;

    [Header("Characters")]
    public byte MaxCharacters;
    public Vector2[] CharacterStartPositions;
    public float CharacterMovementSpeed;
    //Health
    public float MaxHealth;
    //Mana
    public float StartingMaxMana;
    public float EndingMaxMana;
    public float StartingManaRegen;
    public float EndingManaRegen;
    public float ManaScalingTime;
    //Invincibility
    public float InvincibilityTime;
    public float InvincibilityAlphaMod;

    [Header("Controls")]
    [SerializeField] private InputActionNames inputActions;
    public static InputActionNames InputNames => Used.inputActions;

    [Header("Online")]
    [SerializeField][Range(10, 30)] private int NetworkDiscrepancyCheckHz; // reasonable rate is between 30 and 10Hz
    public int NetworkDiscrepancyCheckFrequency => Mathf.CeilToInt (1 / (NetworkDiscrepancyCheckHz * Time.fixedDeltaTime)); // Converts from Hz to number of ticks
    /* I think this was originally for syncing movement stuff, I'm not entirely sure though. leaving it here since it's decently likely I'll want to use it again.
    [SerializeField] private int NetworkTickRateHz; // reasonable rate is probably 50hz
    public int NetworkTickFrequency => Mathf.CeilToInt(1 / (NetworkTickRateHz * Time.fixedDeltaTime)); // Converts from Hz to number of ticks I*/
    public float NetworkLocationDiscrepancyLimit;
    public float NetworkStatBarDiscrepancyLimit;

    [Header("GUI")]
    [SerializeField] private UIPositioning uiPositioning;
    public static UIPositioning UIPositioning => Used.uiPositioning;
    public float StatLostVelocity;
    
    [Header("Cursor")]
    public float CursorMovementSpeed;
    public float CursorAcceleratedMovementMod;
}
