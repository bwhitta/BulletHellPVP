using UnityEngine;

[System.Serializable]
public struct Spellbook
{
    // not sure if this should be a struct or a class - making it a struct might make it so that I can't create it without enetering a value which could be good

    // Constructors
    public Spellbook(byte slots)
    {
        SpellInfos = new SpellData.SpellInfo[slots];
    }
    public Spellbook(SpellData.SpellInfo[] spellInfos)
    {
        SpellInfos = spellInfos;
    }

    // Fields
    public SpellData.SpellInfo[] SpellInfos;

    // Methods
    public readonly string SpellNames()
    {
        string spellNames = "";
        foreach (var spellInfo in SpellInfos)
        {
            spellNames += spellInfo.Spell.name + ", ";
        }
        return spellNames;
    }
    public static Spellbook[] CreateBooks(byte numberOfBooks, byte slots)
    {
        if (numberOfBooks == 0 || slots == 0) Debug.LogError($"invalid number of books or slots per book when creating books! numberOfBooks: {numberOfBooks}, slots: {slots}");

        Spellbook[] books = new Spellbook[numberOfBooks];
        books.Populate(new Spellbook(slots));

        return books;
    }
    public static Spellbook[] CreateBooks(byte numberOfBooks, byte slots, Spellbook firstBookOverride)
    {
        Spellbook[] books = CreateBooks(numberOfBooks, slots);

        if (firstBookOverride.SpellInfos == null) Debug.LogError($"Book given to override first book is empty!");
        Debug.Log($"Overriding first book");
        books[0] = firstBookOverride;

        return books;
    }
}