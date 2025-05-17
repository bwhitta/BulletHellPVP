using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Spell Module")]
public class SpellModule : ScriptableObject
{
    public enum SpawningArea { Point, AdjacentCorners }
    public enum TargetTypes { Opponent, Center, Opposing, InvertedOpposing, NotApplicable }
    public enum MovementTypes { Linear, Wall }
    public enum ModuleTypes { Projectile, PlayerAttached }
    
    /*Hidden*/
    public bool FoldoutOpen;

    public ModuleTypes ModuleType;
    /* -- ModuleType: Projectile -- */
    public SpawningArea ProjectileSpawningArea;
    public TargetTypes TargetingType;
    public MovementTypes MovementType;
    public float MovementSpeed;
    public bool AbilityDealsDamage;
    public float Damage;
    /* -- ModuleType: PlayerAttached -- */
    /* public float AttachmentTime;
    public bool PushesPlayer;
    public float PlayerPushSpeed;
    public bool SpriteFacingPush;
    public bool AngleAfterStart;
    public float AngleChangeSpeed;
    public bool AffectsPlayerMovement;
    public float PlayerMovementMod;*/

    public int InstantiationQuantity;
    public float InstantiationScale;

    public bool UsesCollider;
    public Vector2[] ColliderPath;

    public bool SpellUsesSprite;
    public Sprite UsedSprite;

    public bool Animated;
    public RuntimeAnimatorController AnimatorController;
    public GameObject[] MultipartAnimationPrefabs;
    public float AnimationScaleMultiplier;

    public bool GeneratesParticles;
    public GameObject ParticleSystemPrefab;
    public float ParticleSystemZ;

    public bool ScalesOverTime;
    public float ScalingStartPercent;
    public float MaxScaleMultiplier;
    public bool DestroyOnScalingCompleted;
}