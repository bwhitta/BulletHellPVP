using UnityEngine;

[System.Serializable]
public class SpellModule
{
    public bool FoldoutOpen;

    public enum ModuleTypes { Projectile, PlayerAttached }
    public enum SpawningArea { Point, AdjacentCorners }
    public enum TargetTypes { Opponent, Center, Opposing, InvertedOpposing, NotApplicable }
    public enum MovementTypes { Linear, Wall }
    
    public ModuleTypes ModuleType;
    /* -- ModuleType: Projectile -- */
    public SpawningArea ProjectileSpawningArea; // currently unimplemented
    public TargetTypes TargetingType; // currently unimplemented
    public MovementTypes MovementType; // currently unimplemented
    public float MovementSpeed; // currently unimplemented
    public bool DealsDamage;
    public float Damage;
    /* -- ModuleType: PlayerAttached -- */
    public float AttachmentTime; // currently unimplemented
    /*public bool PushesPlayer;
    public float PlayerPushSpeed;
    public bool SpriteFacingPush;
    public bool AngleAfterStart;
    public float AngleChangeSpeed;
    public bool AffectsPlayerMovement;
    public float PlayerMovementMod;*/

    public int InstantiationQuantity;
    public float InstantiationScale;

    public bool UsesCollider; // currently unimplemented
    public Vector2[] ColliderPath; // currently unimplemented

    public bool SpellUsesSprite;
    public Sprite UsedSprite;

    public bool UsesAnimation;
    public RuntimeAnimatorController AnimatorController;
    public GameObject[] MultipartAnimationPrefabs;
    public float AnimationScaleMultiplier;

    public bool GeneratesParticles;
    public GameObject ParticleSystemPrefab;
    public float ParticleSystemZ;

    public bool ScalesOverTime; // currently unimplemented
    public float ScalingStartMultiplier; // currently unimplemented
    public float MaxScaleMultiplier; // currently unimplemented
    public bool DestroyOnScalingCompleted; // currently unimplemented - shouldn't this only exist if there is something that sets up the duration for how long the scaling should take?
}