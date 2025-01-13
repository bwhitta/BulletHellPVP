using UnityEngine;

[CreateAssetMenu(menuName = "Character Information")]
public class CharacterInfo : ScriptableObject
{
    // [Space(25)] // Spellbook
    /*[SerializeField] private bool overrideFirstBook;
    [SerializeField] private byte[] overrideSetIndexes, overrideSpellIndexes;*/ // DISABLED FOR RESTRUCTURING, maybe also will want to move somewhere else.

    public Vector2 CharacterStartLocation;
    public Vector2 BattleAreaCenter;
    public Vector2 SpellbookPosition;
    public Vector2 HealthBarPosition;
    public Vector2 ManaBarPosition;
}
