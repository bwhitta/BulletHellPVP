using UnityEngine;

[CreateAssetMenu(menuName = "Game Settings")]
public class GameSettings : ScriptableObject
{
    [Space]
    [Header("Spells")]
    public int OffensiveSpellSlots, DefensiveSpellSlots;
    public int TotalBooks;
    public bool CanLoopBooks;
    public int TotalSpellSlots => OffensiveSpellSlots + DefensiveSpellSlots;
    public SpellSetInfo[] SpellSets;

    [Space]
    [Header("Characters")]
    public CharacterInfo[] Characters;
    
    [Space]
    [Header("Online")]
    public int ServerLocationTickFrequency;
    public float ServerClientDiscrepancyLimit;
}
