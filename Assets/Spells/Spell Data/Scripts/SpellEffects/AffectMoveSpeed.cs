using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Spell Effects/Affect Movement Speed")]
public class AffectMoveSpeed : StatusEffect
{
    [SerializeField] private float PlayerMovementMod;

    // Methods
    public override void ApplyEffect(CharacterStatusEffects target)
    {
        target.MoveSpeedModifier *= PlayerMovementMod;
    }
}
