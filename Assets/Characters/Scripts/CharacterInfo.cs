using UnityEngine;

[CreateAssetMenu(menuName = "Character Information")]
public class CharacterInfo : ScriptableObject
{
    // THERE PROBABLY SHOULDN'T BE ANY FIELDS IN THIS SCRIPT THAT ARE MODIFIED AT RUNTIME

    [Space(25)] // Sorting layer
    public string SortingLayer;

    [Space(25)] // Controls (MAYBE SPLIT OFF)
    public string InputMapName;
    public string MovementActionName;
    public string NextBookActionName;
    public string CastingActionName;

    [Space(25)] // Animation
    public string AnimatorTreeParameterX;
    public string AnimatorTreeParameterY;

    [Space(25)] // Spellbook
    /*[SerializeField] private bool overrideFirstBook;
    [SerializeField] private byte[] overrideSetIndexes, overrideSpellIndexes;*/ // DISABLED FOR RESTRUCTURING
    public Spellbook[] EquippedBooks;

    [Space(25)] // Movement
    public Vector2 CharacterStartLocation;

    [Space(25)] // Positioning
    public Vector2 BattleAreaCenter;
    public Vector2 SpellbookPosition;
    public Vector2 HealthBarPos;
    public Vector2 ManaBarPos;
}
