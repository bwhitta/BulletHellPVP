using UnityEngine;

[CreateAssetMenu(menuName = "Game Settings")]
public class GameSettings : ScriptableObject
{
    public static GameSettings Used;

    [Header("Spells")]
    public byte TotalBooks;
    public bool CanLoopBooks;
    public byte SpellSlots;
    public SpellSetInfo[] SpellSets;

    [Header("Play Area")]
    public float BattleSquareWidth;

    [Header("Characters")]
    public byte MaxCharacters;
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

    [Header("Online")]
    [SerializeField][Range(10, 30)] private int NetworkDiscrepancyCheckHz; // reasonable rate is between 30 and 10Hz
    public int NetworkDiscrepancyCheckFrequency => Mathf.CeilToInt (1 / (NetworkDiscrepancyCheckHz * Time.fixedDeltaTime)); // Converts from Hz to number of ticks
    [SerializeField] private int NetworkTickRateHz; // reasonable rate is probably 50hz
    public int NetworkTickFrequency => Mathf.CeilToInt(1 / (NetworkTickRateHz * Time.fixedDeltaTime)); // Converts from Hz to number of ticks
    public float NetworkLocationDiscrepancyLimit;
    public float NetworkStatBarDiscrepancyLimit;
    public int ManaAwaitingTimeLimit;
    
    [Header("GUI")]
    public float StatLostVelocity;
    
    [Header("Cursor")]
    public float CursorMovementSpeed;
    public float CursorAcceleratedMovementMod;
    public string CursorMovementInputName;
    public string AccelerateCursorInputName;
}
