using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Spell")]
public class SpellData : ScriptableObject
{
    public enum SpawningAreas { Point, AdjacentCorners }
    public enum TargetTypes { Character, Center, Opposing, InvertedOpposing, NotApplicable }
    public enum MovementTypes { Linear, Wall }
    public enum ModuleTypes { Projectile, PlayerAttached }


    // Spell Info
    public string SpellName;
    public float ManaCost;
    public float SpellCooldown;
    public Sprite Icon;
    public string Description;

    public int ModuleQuantity;
    public Module[] UsedModules;

    [System.Serializable]
    public class Module
    {
        /*Hidden*/ public bool FoldoutOpen;

        public GameObject Prefab;

        public ModuleTypes ModuleType;
        /* -- ModuleType: Projectile -- */
        public SpawningAreas ProjectileSpawningArea;
        public TargetTypes TargetingType;
        public MovementTypes MovementType;
        public float MovementSpeed;
        public bool AbilityDealsDamage;
        public float Damage;
        /* -- ModuleType: PlayerAttached -- */
        public float AttachmentTime;
        public bool PushesPlayer;
        public float PlayerPushSpeed;
        public bool SpriteFacingPush;
        public bool AngleAfterStart;
        public float AngleChangeSpeed;
        public bool AffectsPlayerMovement;
        public float PlayerMovementMod;
        

        public int InstantiationQuantity; 
        public float InstantiationScale;

        public bool UsesCollider;
        public Vector2[] ColliderPath;

        public bool UsesSprite;
        public Sprite Sprite;

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
}