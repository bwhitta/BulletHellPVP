using Unity.Netcode;
using UnityEngine;

public class Spell : NetworkBehaviour
{
    // Fields
    [HideInInspector] public SpellModule Module;
    public byte ModuleObjectIndex;
    public byte TargetId;
    
    private float lifespan;
    private float distanceMoved;

    private int ticksSincePositionUpdate;
    private readonly NetworkVariable<Vector2> serverSidePosition = new();

    private const float outOfBoundsDistance = 15f;

    // Properties
    private Vector2 _spellLocalPosition;
    private Vector2 SpellLocalPosition
    {
        get
        {
            return _spellLocalPosition;
        }
        set
        {
            _spellLocalPosition = value;
            if (Module.PlayerAttached)
            {
                Vector2 targetPosition = CharacterManager.CharacterTransforms[TargetId].position;
                transform.position = _spellLocalPosition + targetPosition;
            }
            else
            {
                transform.position = _spellLocalPosition;
            }
        }
    }

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
        
        // Set up player local position tracking
        SpellLocalPosition = transform.position;

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
            if (!IsServer)
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
        ScaleSpell();
        CheckBounds();
        if (Module.DestroyAfterDistanceMoved)
        {
            CheckDistanceMoved();
        }
        TickLifespan();
    }

    private void MoveSpell()
    {
        Vector2 movement = new();
        foreach (var spellMovement in Module.SpellMovements)
        {
            movement += spellMovement.Move(transform.eulerAngles.z, ModuleObjectIndex);
        }
        distanceMoved += movement.magnitude * Time.fixedDeltaTime;
        // not 100% positive that this += will work with properties well
        SpellLocalPosition += movement * Time.fixedDeltaTime;
    }
    private void ScaleSpell()
    {
        // Sometimes this array can be null when empty, probably some sort of ScriptableObject compiler jank. It's best to just check to make sure that doesn't happen
        if (Module.SpellScalings == null) return;
        
        float scale = Module.StartingScale;
        foreach(var spellScaling in Module.SpellScalings)
        {
            scale *= spellScaling.Scale(distanceMoved, lifespan);
        }
        transform.localScale = new Vector3(scale, scale, 1);
    }
    private void CheckBounds()
    {
        float distanceFromCenter = Vector2.Distance(transform.position, Vector2.zero);
        if (distanceFromCenter >= outOfBoundsDistance)
        {
            Debug.Log($"Deleted spell - out of bounds");
            DestroySelfNetworkSafe();
        }
    }
    private void CheckDistanceMoved()
    {
        if (distanceMoved >= Module.DestroyDistance)
        {
            DestroySelfNetworkSafe();
        }
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
        SpellLocalPosition = Calculations.DiscrepancyCheck(SpellLocalPosition, newValue, GameSettings.Used.NetworkLocationDiscrepancyLimit);
    }
    void ServerDiscrepancyCheckTick()
    {
        ticksSincePositionUpdate++;
        if (ticksSincePositionUpdate >= GameSettings.Used.NetworkDiscrepancyCheckFrequency)
        {
            serverSidePosition.Value = SpellLocalPosition;
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

    // Old PlayerAttached stuff
    /*private void FixedUpdate()
    {
        // Make sprite face towards where the character is being pushed
        if (Module.SpriteFacingPush)
        {
            var angle = Vector2.SignedAngle(Vector2.up, tempPlayerMovementMod);
            transform.rotation = Quaternion.Euler(0, 0, 180 + angle);
        }
        
        // Local Methods
        if (Module.AngleAfterStart)
        {
            TryAnglingPush();
        }
        
        // what does this even do:
        float GetAngle(Vector2 vector)
        {
            // Returns angle from top, counterclockwise
            return Vector2.SignedAngle(Vector2.up, vector);
        }
    }*/
    /* REMOVED FOR RESTRUCTURING, when re-implementing this will need some extra work done to make the inputs sync with the server
    private void TryAnglingPush()
    {
        Vector2 inputVector = characterControls.movementAction.ReadValue<Vector2>();
        float movingDirection = GetAngle(tempPlayerMovementMod.tempPush);
        float inputDirection = GetAngle(inputVector);
        if (inputVector == Vector2.zero)
            return;
        float movementCap = Module.AngleChangeSpeed * Time.fixedDeltaTime;
        float rotationAngle = Mathf.MoveTowardsAngle(movingDirection, inputDirection, movementCap);
        // Change the push direction to still move the player 
        tempPlayerMovementMod.tempPush = Quaternion.Euler(0, 0, rotationAngle) * Vector2.up;
    }*/
}
