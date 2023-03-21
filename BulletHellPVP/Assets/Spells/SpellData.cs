using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Spell")]
public class SpellData : ScriptableObject
{
    public enum SpawningAreas { Points, AdjacentCorners }
    public enum TargetTypes { Character, Center, Opposing, InvertedOpposing, NotApplicable }
    public enum MovementTypes { Linear, Wall }

    // Basic Spell Info
    public string SpellName;
    public float ManaCost;
    public float SpellCooldown;
    public GameObject Prefab;
    public int InstantiationQuantity;

    public Sprite Icon;
    public string Description;
    
    public bool SpellUsesMovement = false;
        public SpawningAreas SpawningArea;
        public TargetTypes TargetingType;
        public MovementTypes MovementType;
        public float MovementSpeed;

    public bool SpellDealsDamage;
        public float Damage;

    public bool SpellUsesCollider;
        public Vector2[] ColliderPath;

    public bool SpellUsesSprite;
        public Sprite SpellSprite;
        public float SpriteScale;

    public bool AnimatedSpell;
        public RuntimeAnimatorController SpellAnimatorController;
        public GameObject[] MultipartAnimationPrefabs;

    public bool SpellScales;
        public float ScalingStartPercent;
        public float MaxScaleMultiplier;
        public bool DestroyOnScalingCompleted;
}