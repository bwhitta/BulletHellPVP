using Unity.Netcode;
using UnityEngine;

public class SpellModuleBehavior : NetworkBehaviour
{
    #region Fields
    public SpellData.Module Module
    {
        get
        {
            SpellSetInfo set = GameSettings.Used.SpellSets[setIndex];
            SpellData spell = set.spellsInSet[spellIndex];
            return spell.UsedModules[moduleIndex];
        }
    }
    public SpellData ModuleSpellData
    {
        get
        {
            SpellSetInfo set = GameSettings.Used.SpellSets[spellIndex];
            SpellData spell = set.spellsInSet[spellIndex];
            return spell;
        }
    }
    public byte setIndex, spellIndex, moduleIndex, behaviorId, ownerId;

    // Projectile
    private float distanceMoved;

    // Player Attached
    private float attachmentTime;
    private CharacterControls characterControls;

    // Temporary movement modification
    CharacterControls.TempMovementMod tempPlayerMovementMod;

    // Display
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    // Network Variables
    private readonly NetworkVariable<Vector2> serverSidePosition = new();
    private int ticksSincePositionUpdate;

    // Character Information

    // Information for online sync
    public float cursorLocationOnCast;

    // Readonlys
    private readonly float outOfBoundsDistance = 15f;
    #endregion
    
    // Methods
    private void Start()
    {
        // Error logging
        if (Module == null)
        {
            Debug.LogError($"Module null", this);
        }

        // If local or server set cursor position on cast
        if (!MultiplayerManager.IsOnline || IsServer)
        {
            // probably should rework this anyways so that it only tracks the cursor's position on cast if it matters (if it tracks it at all)
            //cursorPositionOnCast = OwnerCharacterInfo.CursorLogicScript.location; // DISABLED FOR RESTRUCTURING
        }
        
        // Send data to client
        if (IsServer)
        {
            Debug.Log($"Sending module data to clients");
            ModuleDataClientRpc(setIndex, spellIndex, moduleIndex, behaviorId, ownerId, cursorLocationOnCast);
        }
        
        // Set variables
        SetStartingPosition();

        // Sets up targeting
        if (Module.ModuleType == SpellData.ModuleTypes.Projectile)
        {
            PointTowardsTarget();
        }
        SetScale();

        // Attach module (if applicable)
        if (Module.ModuleType == SpellData.ModuleTypes.PlayerAttached)
        {
            // Set up parenting
            //transform.parent = OwnerCharacterInfo.CharacterObject.transform; DISABLED FOR RESTRUCTURING
            //transform.localPosition = Vector2.zero;

            // Set how long the spell should last
            attachmentTime = Module.AttachmentTime;

            // Set up a reference to the character controls
            characterControls = transform.parent.GetComponent<CharacterControls>();
            
        }

        // Enable the optional parts of the module
        if (Module.UsesSprite)
        {
            EnableSprite();
        }
        if (Module.Animated)
        {
            EnableAnimator();
        }
        if (Module.GeneratesParticles)
        {
            EnableParticleSystem();
        }
        if (Module.UsesCollider)
        {
            GetComponent<PolygonCollider2D>().enabled = true;
            SetCollider();
        }
        if (Module.AffectsPlayerMovement)
        {
            if (Module.ModuleType == SpellData.ModuleTypes.PlayerAttached)
            {
                ModifyPlayerMovement();
            }
            else Debug.LogWarning("Module is attempting to affect player movement on a non-attached spell");
        }
        
        // Sync server position for clients
        if (MultiplayerManager.IsOnline && !IsServer) serverSidePosition.OnValueChanged += ServerPositionUpdate;
        
        // Local Methods
        void ServerPositionUpdate(Vector2 oldValue, Vector2 newValue)
        {
            transform.position = Calculations.DiscrepancyCheck(transform.position, newValue, GameSettings.Used.NetworkLocationDiscrepancyLimit);
        }
        void SetStartingPosition()
        {
            switch (Module.ProjectileSpawningArea)
            {
                case SpellData.SpawningAreas.Point:
                    // transform.position = CursorMovement.CalculateCursorTransform(cursorPositionOnCast, OwnerCharacterInfo.OpponentAreaCenter); 
                    break;
                case SpellData.SpawningAreas.AdjacentCorners:
                    // Turns the float position of the cursor into a rotation.
                    int side = CursorMovement.SquareSideAtPosition(GameSettings.Used.BattleSquareWidth, cursorLocationOnCast);
                    Quaternion cursorAngleOnCast = Quaternion.Euler(0, 0, (-90 * side) - 90); // no clue why I use -90 and not 90 here, but that's what I did for other parts of the code so I won't question it.                    
                    // Sets the position and rotation
                    // transform.SetPositionAndRotation(AdjacentCornersPos(), cursorAngleOnCast);  DISABLED FOR RESTRUCTURING
                    break;
                default:
                    Debug.LogWarning("Not yet implemented spawning area!");
                    break;
            }
            
            /*Vector2 AdjacentCornersPos()
            {
                int side = CursorMovement.SquareSideAtPosition(GameSettings.Used.BattleSquareWidth, cursorLocationOnCast);
                Vector2[] corners = Calculations.GetSquareCorners(GameSettings.Used.BattleSquareWidth, OwnerCharacterInfo.OpponentAreaCenter);

                // Points to instantiate at
                Vector2[] spawnPoints = new Vector2[]
                {
                        corners[side],
                        corners[(side + 1) % 4]
                };
                return spawnPoints[behaviorId];
            } */ // DISABLED FOR RESTRUCTURING
        }
        void SetScale()
        {
            transform.localScale = new Vector3(Module.InstantiationScale, Module.InstantiationScale, 1);
        } 
        void EnableSprite()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.enabled = Module.UsesSprite;
            spriteRenderer.sprite = Module.UsedSprite;

            // Set the mask layer
            // string spellMaskLayer; DISABLED FOR RESTRUCTURING
            if (Module.ModuleType == SpellData.ModuleTypes.PlayerAttached)
            {
                // spellMaskLayer = OwnerCharacterInfo.SortingLayer; DISABLED FOR RESTRUCTURING
            }
            else
            {
                // spellMaskLayer = OwnerCharacterInfo.OpponentCharacterInfo.CharacterAndSortingTag; DISABLED FOR RESTRUCTURING
            }
            // spriteRenderer.sortingLayerName = spellMaskLayer; DISABLED FOR RESTRUCTURING
        }
        void EnableAnimator()
        {
            animator = GetComponent<Animator>();
            foreach (GameObject animationPrefab in Module.MultipartAnimationPrefabs)
            {
                // Spawn in the animator
                GameObject currentAnimationPrefab = Instantiate(animationPrefab, transform);
                currentAnimationPrefab.transform.SetPositionAndRotation(transform.position, transform.rotation);
                currentAnimationPrefab.transform.localScale = new Vector2(Module.AnimationScaleMultiplier, Module.AnimationScaleMultiplier);
                
                // Animator does not work with changed name, so this line resets the name.
                currentAnimationPrefab.name = animationPrefab.name;

                // Make sure the sprite shows up on your own side of the play area when it is attached to yourself 
                // string spellMaskLayer; DISABLED FOR RESTRUCTURING
                if (Module.ModuleType == SpellData.ModuleTypes.PlayerAttached)
                {
                    // spellMaskLayer = OwnerCharacterInfo.SortingLayer; DISABLED FOR RESTRUCTURING
                }
                else
                {
                    // spellMaskLayer = OwnerCharacterInfo.OpponentCharacterInfo.CharacterAndSortingTag; DISABLED FOR RESTRUCTURING
                }
                
                // Set the mask layer
                // currentAnimationPrefab.GetComponent<SpriteRenderer>().sortingLayerName = spellMaskLayer; DISABLED FOR RESTRUCTURING
            }

            // Enables the animator if Animated is set to true
            animator.enabled = Module.Animated;

            // Sets the animation
            animator.runtimeAnimatorController = Module.AnimatorController;
        }
        void EnableParticleSystem()
        {
            GameObject particleObject = Instantiate(Module.ParticleSystemPrefab, transform);
            particleObject.transform.localPosition = new Vector3(0, 0, Module.ParticleSystemZ);
        }
        void SetCollider()
        {
            GetComponent<PolygonCollider2D>().points = Module.ColliderPath;
        }
        void ModifyPlayerMovement()
        {
            // Creates the tempMovementMod in the character controls script
            tempPlayerMovementMod = new CharacterControls.TempMovementMod();
            
            if (Module.PushesPlayer)
            {
                // Detect the inputs
                Vector2 inputDirection;
                inputDirection = characterControls.movementAction.ReadValue<Vector2>().normalized;
                
                // If the player is stationary, send down
                if (inputDirection == Vector2.zero)
                {
                    inputDirection = new Vector2(0, -1);
                }

                // Send the push vector to character controls, with a magnitude equal to the speed of the 
                Debug.LogWarning("might need to add something here later as a ServerRPC telling the server which direction was inputted, delete me later");
                tempPlayerMovementMod.tempPush = inputDirection * Module.PlayerPushSpeed;
            }
            if (Module.AffectsPlayerMovement)
            {
                tempPlayerMovementMod.tempMovementMod = Module.PlayerMovementMod;
            }
            
            characterControls.tempMovementMods.Add(tempPlayerMovementMod);
        }
    }
    private void FixedUpdate()
    {
        if (IsServer) ServerPositionTick();

        switch (Module.ModuleType)
        {
            case SpellData.ModuleTypes.Projectile:
                MoveSpell();
                break;
            case SpellData.ModuleTypes.PlayerAttached:
                PlayerAttachedTick();
                break;
        }
        // Delete if too far away
        CheckBounds();
        void CheckBounds()
        {
            float distanceFromCenter = Vector2.Distance(transform.position, Vector2.zero);
            if (distanceFromCenter >= outOfBoundsDistance)
            {
                Debug.Log($"Deleted - out of bounds");
                DestroySelfNetworkSafe();
            }
        }
    }

    private void PlayerAttachedTick()
    {
        // Attatchment Time
        attachmentTime--;
        if (attachmentTime <= 0)
        {
            if (tempPlayerMovementMod != null)
            {
                // Remove the tempMovementMod at the end of the current frame
                tempPlayerMovementMod.removeEffect = true;
            }
            DestroySelfNetworkSafe();
        }
        
        // Make sprite face towards where the character is facing
        if (Module.SpriteFacingPush)
        {
            var angle = Vector2.SignedAngle(Vector2.up, tempPlayerMovementMod.tempPush);
            transform.rotation = Quaternion.Euler(0, 0, 180 + angle);
        }
        
        /* Currently removed but might re-add later, though the inputs will need to be synced with the server somehow
        // Local Methods
        if (Module.AngleAfterStart)
        {
            TryAnglingPush();
        }
        void TryAnglingPush() 
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
        }
        float GetAngle(Vector2 vector)
        {
            // Returns angle from top, counterclockwise
            return Vector2.SignedAngle(Vector2.up, vector);
        }
        */
    }
    private void ServerPositionTick()
    {
        // Discrepancy checks
        ticksSincePositionUpdate++;
        if (ticksSincePositionUpdate >= GameSettings.Used.NetworkDiscrepancyCheckFrequency)
        {
            serverSidePosition.Value = transform.position;
            ticksSincePositionUpdate = 0;
        }
    }

    private void PointTowardsTarget()
    {
        // If TargetingType is CharacterStats, point towards the character
        switch (Module.TargetingType)
        {
            case SpellData.TargetTypes.Opponent:
                //GameObject opponent = OwnerCharacterInfo.OpponentCharacterInfo.CharacterObject; DISABLED FOR RESTRUCTURING
                //transform.right = opponent.transform.position - transform.position; // Point towards character DISABLED FOR RESTRUCTURING
                break;
            case SpellData.TargetTypes.NotApplicable:
                //Do nothing
                return;
            default:
                Debug.LogWarning("Targeting type is not yet implemented.");
                break;
        }
    }
    private void MoveSpell()
    {
        // Move the spell
        if (Module.MovementType == SpellData.MovementTypes.Linear)
        {
            transform.position += Module.MovementSpeed * Time.fixedDeltaTime * transform.right;
            distanceMoved += Module.MovementSpeed * Time.fixedDeltaTime;
        }
        else if (Module.MovementType == SpellData.MovementTypes.Wall)
        {
            switch (behaviorId)
            {
                case 0:
                    transform.position += transform.rotation * Vector3.up * Time.fixedDeltaTime * Module.MovementSpeed;
                    break;
                case 1:
                    transform.position += transform.rotation * Vector3.down * Time.fixedDeltaTime * Module.MovementSpeed;
                    break;
                default:
                    Debug.LogWarning($"behaviorId {behaviorId} should not be possible in this situation.");
                    break;
            }
            distanceMoved += Time.fixedDeltaTime * Module.MovementSpeed;
        }

        // Scaling
        if (Module.ScalesOverTime)
            UpdateScaling();
    }
    private void UpdateScaling()
    {
        float distanceToMove = GameSettings.Used.BattleSquareWidth / 2;
        float distanceForScaling = distanceToMove * Module.ScalingStartPercent;

        if (distanceMoved >= distanceForScaling)
        {
            float currentScale = Scaling(distanceToMove, Module.ScalingStartPercent, distanceMoved, Module.MaxScaleMultiplier - 1);
            transform.localScale = new Vector3(Module.InstantiationScale * currentScale, Module.InstantiationScale * currentScale, 1);
        }

        if (distanceMoved >= distanceToMove && Module.DestroyOnScalingCompleted)
        {
            Debug.Log($"Fully moved, destroying {name}. Distance moved: {distanceMoved}. Distance to move: {distanceToMove}");
            DestroySelfNetworkSafe();
        }

    }
    private float Scaling(float totalMove, float totalMoveScalingStartPercent, float currentlyMoved, float scaleTargetPercentage)
    {
        // The position along totalMove at which scaling starts
        float scalingStart = totalMove * totalMoveScalingStartPercent;
        // The percentage (0.0 to 1.0) of scaling completed
        float scalingCompletionPercentage = (currentlyMoved - scalingStart) / (totalMove - scalingStart);
        // Cap at 1.0 (100%)
        scalingCompletionPercentage = Mathf.Min(scalingCompletionPercentage, 1f);

        return (scaleTargetPercentage * scalingCompletionPercentage) + 1f;
    }
    
    private void DestroySelfNetworkSafe()
    {
        if (!MultiplayerManager.IsOnline)
        {
            Debug.Log($"Destroying {gameObject.name}.");
            Destroy(gameObject);
        }
        else if (IsServer)
        {
            Debug.Log($"Destroying {gameObject.name} as online server.");
            NetworkObject.Despawn(gameObject);
        }
        else
        {
            Debug.Log($"Disabling {gameObject.name} until it is destroyed by server.");
            gameObject.SetActive(false);
        }
    }

    [ClientRpc]
    private void ModuleDataClientRpc(byte serverSetIndex, byte serverSpellIndex, byte serverModuleIndex, byte serverBehaviorId, byte serverOwnerId, float serverCursorPositionOnCast)
    {
        if (IsHost)
        {
            return;
        }
        setIndex = serverSetIndex;
        spellIndex = serverSpellIndex;
        moduleIndex = serverModuleIndex;
        behaviorId = serverBehaviorId;
        ownerId = serverOwnerId;
        cursorLocationOnCast = serverCursorPositionOnCast;
        // Only deduct Mana Awaiting if this is the first SpellModuleBehavior
        if (behaviorId == 0)
        {
            // OwnerCharacterInfo.Stats.ManaAwaiting -= ModuleSpellData.ManaCost; DISABLED FOR RESTRUCTURING, also I don't get how this even works. Is the spawned spell seriously responsible for deducting the mana?
        }
        Debug.Log($"This client recieved data from the server!\n(data was - setIndex: {setIndex}, spellIndex: {spellIndex}, moduleIndex: {moduleIndex}, behaviorId: {behaviorId}, ownerId: {ownerId}, serverCursorPositionOnCast: {serverCursorPositionOnCast})");
    }
}