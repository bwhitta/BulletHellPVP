using UnityEngine;

[CreateAssetMenu(menuName = "Game Settings")]
public class GameSettings : ScriptableObject
{
    public int OffensiveSpellSlots, DefensiveSpellSlots;
    public int TotalSpellSlots => OffensiveSpellSlots + DefensiveSpellSlots;
    public SpellSetInfo[] SpellSets;
    public CharacterInfo[] Characters;
}
