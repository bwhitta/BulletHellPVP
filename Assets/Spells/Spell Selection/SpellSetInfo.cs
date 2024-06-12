using UnityEngine;

[CreateAssetMenu(menuName = "Spell Sets")]
public class SpellSetInfo : ScriptableObject
{
    public Sprite SetSprite;
    public SpellData[] spellsInSet;
}
