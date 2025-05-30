using UnityEngine;

public abstract class SpellScaling : ScriptableObject
{
    // Methods
    public abstract float Scale(float distanceMoved, float spellLifespan);
}
