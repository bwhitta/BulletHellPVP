using UnityEngine;

[CreateAssetMenu(menuName = "Game Settings")]
public class GameSettings : ScriptableObject
{
    public static GameSettings Used;

    [Space]
    [Header("Spells")]
    public byte OffensiveSpellSlots, DefensiveSpellSlots;
    public byte TotalBooks;
    public bool CanLoopBooks;
    public int TotalSpellSlots => OffensiveSpellSlots + DefensiveSpellSlots;
    public SpellSetInfo[] SpellSets;

    [Space]
    [Header("Play Area")]
    public float BattleSquareWidth;

    [Space]
    [Header("Characters")]
    public CharacterInfo[] Characters;
    public float CharacterMovementSpeed;

    [Space]
    [Header("Character Stats")]
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

    [Space]
    [Header("Online")]
    [Range(10, 30)] public int ServerLocationHz; // reasonable rate is between 30 and 10Hz (I'll probably want something in between, and to add the option to choose in the settings)
    public int ServerLocationTickFrequency => Mathf.CeilToInt(1 / (ServerLocationHz * Time.fixedDeltaTime)); // Converts from Hz to number of ticks

    public float ServerClientDiscrepancyLimit;

    [Space]
    [Header("GUI")]
    public float StatLostVelocity;
    
    [Space]
    [Header("Cursor")]
    public float CursorMovementSpeed;
    public float CursorAcceleratedMovementMod;
    public string CursorMovementInputName;
    public string AccelerateCursorInputName;
}
