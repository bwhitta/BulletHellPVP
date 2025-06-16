public class StatusEffectInstance
{
    // Constructors
    public StatusEffectInstance (StatusEffect effect)
    {
        baseEffect = effect;
    }

    // Fields
    public StatusEffect baseEffect;
    public int EffectAge = 0;

    public void ApplyEffect(CharacterStatusEffects target)
    {
        baseEffect.ApplyEffect(target);
    }
}
