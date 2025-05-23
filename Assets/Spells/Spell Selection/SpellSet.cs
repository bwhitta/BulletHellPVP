using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Spell Set")]
public class SpellSet : ScriptableObject
{
    public Sprite SetSprite;
    public SpellData[] spellsInSet;
}