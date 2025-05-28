using System;
using System.Dynamic;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public class SpellModule
{
    public enum SpellTargets { Owner, Opponent }
    
    // Fields
    public int InstantiationQuantity;
    public SpellStartingPosition StartingPosition;
    public SpellStartingRotation StartingRotation;
    public float StartingScale;

    public SpellMovement[] SpellMovements;
    public SpellTargets SpellTarget;
    
    public bool PlayerAttached;
    /*public bool PushesPlayer;
    public float PlayerPushSpeed;
    public bool SpriteFacingPush;
    public bool AngleAfterStart;
    public float AngleChangeSpeed;
    public bool AffectsPlayerMovement;
    public float PlayerMovementMod;*/

    public bool LimitedLifespan;
    public float Lifespan;

    public bool UsesCollider; //
    public Vector2[] ColliderPath; //

    public bool DealsDamage; // maybe change out for something like a ScriptableObject[] called EffectsOnCollision? could have inheretied SOs like DealsDamage and MovesCharacter and such. the SOs could also just be called Effects, with EffectsOnCollision being a list of effects.
    public float Damage;

    public bool SpellUsesSprite;
    public Sprite UsedSprite;

    public bool UsesAnimation;
    public RuntimeAnimatorController AnimatorController;
    public GameObject[] AnimationPrefabs;
    public float AnimationScaleMultiplier;

    public bool GeneratesParticles;
    public GameObject ParticleSystemPrefab;

    // unimplemented
    public bool ScalesOverTime;
    public float ScalingStartMultiplier;
    public float MaxScaleMultiplier;
    public bool DestroyOnScalingCompleted; // currently unimplemented - shouldn't this only exist if there is something that sets up the duration for how long the scaling should take? for examle, could instead destroy after moving a certain amount (for Fiery Raze this would be half of the battle area)

    // Value type that can be used to get a module (so that it can be sent through networking)
    public struct ModuleInfo : INetworkSerializeByMemcpy
    {
        // Constructors
        public ModuleInfo(byte setIndex, byte spellIndex, byte moduleIndex)
        {
            ModuleSpellData = new(setIndex, spellIndex);
            ModuleIndex = moduleIndex;
        }
        public ModuleInfo(SpellData.SpellInfo spellDataInfo, byte moduleIndex)
        {
            ModuleSpellData = spellDataInfo;
            ModuleIndex = moduleIndex;
        }

        // Fields
        public SpellData.SpellInfo ModuleSpellData;
        public byte ModuleIndex;

        // Properties
        public readonly SpellModule Module
        {
            get
            {
                return ModuleSpellData.Spell.UsedModules[ModuleIndex];
            }
        }
    }
}