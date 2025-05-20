using UnityEngine;

[CreateAssetMenu(menuName = "Spell Set")]
public class SpellSetInfo : ScriptableObject
{
    public Sprite SetSprite;
    public SpellData[] spellsInSet;
}