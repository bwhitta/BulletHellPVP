using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class Spell : NetworkBehaviour
{
    // Fields
    public SpellModule Module;
    public byte ModuleObjectIndex;
    public byte TargetId;
    
    private float lifespan;

    private int ticksSincePositionUpdate;
    private readonly NetworkVariable<Vector2> serverSidePosition = new();

    // Methods
    void Start()
    {
        string maskLayer = GameSettings.Used.spellMaskLayers[TargetId];

        // Set up visuals
        if (Module.SpellUsesSprite)
        {
            SpellVisuals.EnableSprite(GetComponent<SpriteRenderer>(), Module.UsedSprite, maskLayer);
        }
        if (Module.UsesAnimation)
        {
            SpellVisuals.EnableAnimator(GetComponent<Animator>(), transform, Module.AnimatorController, Module.AnimationPrefabs, Module.AnimationScaleMultiplier, maskLayer);
        }
        if (Module.GeneratesParticles)
        {
            SpellVisuals.EnableParticleSystem(transform, Module.ParticleSystemPrefab, maskLayer);
        }
        SpellVisuals.StartingScale(transform, Module.StartingScale);

        // Set up collision
        EnableCollider();

        // Set up networking
        if (MultiplayerManager.IsOnline)
        {
            SetupSpellNetworking();
        }

        // Local Methods
        void EnableCollider()
        {
            PolygonCollider2D collider = GetComponent<PolygonCollider2D>();
            collider.enabled = true;
            collider.points = Module.ColliderPath;
        }
        void SetupSpellNetworking()
        {
            if (IsServer)
            {
                // will add stuff here soon

                // should either have this here or in the SpellSpawner script.
                //ModuleDataClientRpc(SetIndex, SpellIndex, ModuleIndex, ModuleObjectIndex, OwnerId, CursorLocationOnCast);
            }
            else
            {
                serverSidePosition.OnValueChanged += ServerPositionChanged;
            }
        }
    }

    void FixedUpdate()
    {
        if (MultiplayerManager.IsOnline && IsServer)
        {
            ServerDiscrepancyCheckTick(); // rename probably
        }

        MoveSpell();
        // check for out of bounds
        TickLifespan();
    }

    

    private void MoveSpell()
    {
        Vector2 movement = new();
        foreach (var spellMovement in Module.SpellMovements)
        {
            movement += spellMovement.Move(transform.eulerAngles.z, ModuleObjectIndex);
        }
        transform.localPosition += (Vector3)movement * Time.deltaTime;
    }
    private void TickLifespan()
    {
        lifespan++;
        if (Module.LimitedLifespan && lifespan >= Module.Lifespan)
        {
            DestroySelfNetworkSafe();
        }
    }

    // Networking
    void ServerPositionChanged(Vector2 oldValue, Vector2 newValue)
    {
        Debug.Log($"checking server position");
        transform.position = Calculations.DiscrepancyCheck(transform.position, newValue, GameSettings.Used.NetworkLocationDiscrepancyLimit);
    }
    void ServerDiscrepancyCheckTick()
    {
        ticksSincePositionUpdate++;
        if (ticksSincePositionUpdate >= GameSettings.Used.NetworkDiscrepancyCheckFrequency)
        {
            serverSidePosition.Value = transform.position;
            ticksSincePositionUpdate = 0;
        }
    }
    public void DestroySelfNetworkSafe()
    {
        if (!MultiplayerManager.IsOnline)
        {
            Destroy(gameObject);
        }
        else if (IsServer)
        {
            NetworkObject.Despawn(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    [ClientRpc]
    public void ModuleDataClientRpc(SpellModule.ModuleInfo moduleInfo, byte moduleObjectIndex, byte targetId)
    {
        if (IsHost)
        {
            return;
        }

        Debug.Log($"SETTING MODULE INFO!!!");
        SetModuleData(moduleInfo, moduleObjectIndex, targetId);

        /*Only deduct Mana Awaiting if this is the first SpellModuleBehavior
        if (BehaviorIndex == 0)
        {
            OwnerCharacterInfo.Stats.ManaAwaiting -= ModuleSpellData.ManaCost;
        } REMOVED FOR RESTRUCTURING, also I don't get how this even works. Is the spawned spell seriously responsible for deducting the mana?*/
    }
    public void SetModuleData(SpellModule.ModuleInfo moduleInfo, byte moduleObjectIndex, byte targetId)
    {
        Module = moduleInfo.Module;
        ModuleObjectIndex = moduleObjectIndex;
        TargetId = targetId;
    }
}
