using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Scriptable Spell")]
public class SpellData : ScriptableObject
{
    // Spell Info
    public float ManaCost;
    public float SpellCooldown;
    public Sprite Icon;

    public SpellModule[] UsedModules;
}