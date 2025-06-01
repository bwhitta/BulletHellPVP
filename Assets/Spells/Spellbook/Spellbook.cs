using Unity.Netcode;
using UnityEngine;

public struct Spellbook : INetworkSerializeByMemcpy
{
    // Constructors
    public Spellbook(byte slots)
    {
        SetIndexes = new byte[slots];
        SpellIndexes = new byte[slots];
    }

    // Fields
    public byte[] SetIndexes;
    public byte[] SpellIndexes;

    

    // Methods
    public static Spellbook[] CreateBooks(byte numberOfBooks, byte slots)
    {
        if (numberOfBooks == 0 || slots == 0) Debug.LogError($"invalid number of books or slots per book when creating books! numberOfBooks: {numberOfBooks}, slots: {slots}");

        Spellbook[] books = new Spellbook[numberOfBooks];
        for (int i = 0; i < books.Length; i++)
        {
            books[i] = new Spellbook(slots);
        }
        
        return books;
    }
    public static Spellbook[] CreateBooks(byte numberOfBooks, byte slots, Spellbook firstBookOverride)
    {
        if (numberOfBooks == 0 || slots == 0) Debug.LogError($"invalid number of books or slots per book when creating books! numberOfBooks: {numberOfBooks}, slots: {slots}");

        Spellbook[] books = new Spellbook[numberOfBooks];
        for (int i = 0; i < books.Length; i++)
        {
            books[i] = new Spellbook(slots);
        }

        Debug.Log($"Overriding first book");
        books[0] = firstBookOverride;

        return books;
    }
    public static SpellData GetSpellData(byte setIndex, byte spellIndex)
    {
        SpellSet set = GameSettings.Used.SpellSets[setIndex];
        return set.spellsInSet[spellIndex];
    }
    public SpellData SpellInSlot(byte slotIndex)
    {
        byte setIndex = SetIndexes[slotIndex];
        byte spellIndex = SpellIndexes[slotIndex];

        SpellSet set = GameSettings.Used.SpellSets[setIndex];
        return set.spellsInSet[spellIndex];
    }

    // used for sending through networkvariable
    public struct CharacterSpellbooks : INetworkSerializeByMemcpy
    {
        public CharacterSpellbooks(Spellbook[] spellbooks)
        {
            Spellbooks = spellbooks;
        }

        public Spellbook[] Spellbooks;
    }
}