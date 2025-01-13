public class Spellbook
{
    // Since this is very simple, it can probably be part of another script rather than it's own.
    public byte[] SetIndexes;
    public byte[] SpellIndexes;

    public static Spellbook[] CreateBooks(byte slots)
    {
        Spellbook[] books = new Spellbook[slots];
        for (int i = 0; i < slots; i++)
        {
            books[i] = new()
            {
                SetIndexes = new byte[slots],
                SpellIndexes = new byte[slots]
            };
        }
        return books;
    }
}
