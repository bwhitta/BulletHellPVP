using System.Collections.Generic;
using UnityEngine;

public class CharacterStatusEffects : MonoBehaviour
{
    // Fields
    [HideInInspector] public float MoveSpeedModifier = 1f;
    [HideInInspector] public List<StatusEffectInstance> StatusEffects = new();

    // Methods
    private void FixedUpdate()
    {
        ResetEffects();
        for (int i = 0; i < StatusEffects.Count; i++)
        {
            RunEffect(i);
        }
    }

    private void RunEffect(int effectIndex)
    {
        StatusEffectInstance effect = StatusEffects[effectIndex];

        if (effect.EffectAge >= effect.baseEffect.MaxEffectDuration)
        {
            // could this theoretically cause an error or remove the wrong effects if two effects were removed at once?
            StatusEffects.RemoveAt(effectIndex);
            Debug.Log($"removing effect");
        }
        effect.EffectAge++;

        effect.ApplyEffect(this);
    }
    private void ResetEffects()
    {
        MoveSpeedModifier = 1f;
    }
}
