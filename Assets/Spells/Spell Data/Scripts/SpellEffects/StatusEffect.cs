using UnityEngine;

public abstract class StatusEffect : ScriptableObject
{
    //Fields
    public int MaxEffectDuration;
    
    // Methods
    public abstract void ApplyEffect(CharacterStatusEffects target);
}
