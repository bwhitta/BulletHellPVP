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

    [Space(25)] // Equipped Spells (SPLIT OFF MOST OF THIS)
    public byte CurrentBookIndex; // PROBABLY SHOULDN'T BE PART OF CHARACTERINFO
    [SerializeField] private bool overrideFirstBook;
    [SerializeField] private byte[] overrideSetIndexes, overrideSpellIndexes;
    public Spellbook[] EquippedBooks;
    public Spellbook CurrentBook => EquippedBooks[CurrentBookIndex];
    public class Spellbook
    {
        public byte[] SetIndexes;
        public byte[] SpellIndexes;
    }

    [Space(25)] // Movement
    public Vector2 CharacterStartLocation;

    [Space(25)] // Positioning
    public Vector2 OpponentAreaCenter;
    public Vector2 SpellbookPosition;

    [Space(25)] // Stat bars
    public Vector2 healthBarPos;
    public Vector2 manaBarPos;

    // honestly having this in CharacterInfo is a bit of a stretch
    public void CreateBooks()
    {
        if (EquippedBooks != null)
        {
            return;
        }

        // Debug.Log($"Equipped books found null, creating new spellbooks");
        EquippedBooks = new Spellbook[GameSettings.Used.TotalBooks];
        
        for (int i = 0; i < EquippedBooks.Length; i++)
        {
            if(i == 0 && overrideFirstBook)
            {
                EquippedBooks[i] = new()
                {
                    SetIndexes = overrideSetIndexes,
                    SpellIndexes = overrideSpellIndexes
                };
                continue;
            }
            EquippedBooks[i] = new()
            {
                SetIndexes = new byte[GameSettings.Used.TotalSpellSlots],
                SpellIndexes = new byte[GameSettings.Used.TotalSpellSlots]
            };
        }
    }
}
