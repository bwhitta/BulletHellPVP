using UnityEngine;

[System.Serializable]
public class SpellModule
{
    // could adjust AdjacentCorners to work with any number of spells (or at least up to four)
    public enum SpellTargets { Owner, Opponent }
    
    // Fields
    public SpellStartingPosition StartingPosition;
    public SpellStartingRotation StartingRotation;
    public int InstantiationQuantity;
    public float InstantiationScale;

    public SpellMovement[] SpellMovements;
    public SpellTargets SpellTarget;

    public bool PlayerAttached;
    public float AttachmentTime;
    /*public bool PushesPlayer;
    public float PlayerPushSpeed;
    public bool SpriteFacingPush;
    public bool AngleAfterStart;
    public float AngleChangeSpeed;
    public bool AffectsPlayerMovement;
    public float PlayerMovementMod;*/
    
    public bool UsesCollider;
    public Vector2[] ColliderPath;

    public bool DealsDamage; // maybe change out for something like a scriptableObject called EffectsOnCollision? could have inheretied SOs like DealsDamage and MovesCharacter and such. the SOs could also just be called Effects, with EffectsOnCollision being a list of effects.
    public float Damage;

    public bool SpellUsesSprite;
    public Sprite UsedSprite;

    public bool UsesAnimation;
    public RuntimeAnimatorController AnimatorController;
    public GameObject[] MultipartAnimationPrefabs;
    public float AnimationScaleMultiplier;

    public bool GeneratesParticles;
    public GameObject ParticleSystemPrefab;
    public float ParticleSystemZ;

    public bool ScalesOverTime;
    public float ScalingStartMultiplier;
    public float MaxScaleMultiplier;
    public bool DestroyOnScalingCompleted; // currently unimplemented - shouldn't this only exist if there is something that sets up the duration for how long the scaling should take?
}