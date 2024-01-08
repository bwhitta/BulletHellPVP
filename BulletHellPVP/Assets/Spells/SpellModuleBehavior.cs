using Unity.Netcode;
using UnityEngine;

public class SpellModuleBehavior : NetworkBehaviour
{
    #region Fields
    private SpellData.Module module;
    public byte setIndex, spellIndex, moduleIndex, behaviorID, ownerID;

    // Projectile
    public GameObject targetedCharacter;
    private float distanceMoved;

    // Player Attached
    private float attachmentTime;
    private CharacterControls characterControls;
    private Vector2 movementDirection;

    // Display
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    // Network Variables
    private readonly NetworkVariable<Vector2> serverSidePosition = new();
    private int ticksSincePositionUpdate;


    // Readonlys
    private readonly float outOfBoundsDistance = 15f;
    #endregion

    void Start()
    {
        Debug.Log($"Module would be started here. ownerID: {ownerID}, owner character: {GameSettings.Used.Characters[ownerID]}");
        StartModule();
    }
    private void StartModule()
    {
        Debug.Log($"Starting module! OwnerID: {ownerID}, Owner character: {GameSettings.Used.Characters[ownerID]}");

        if (IsServer)
        {
            Debug.Log($"Sending module data to clients (calling ModuleDataClientRpc)");
            ModuleDataClientRpc(setIndex, spellIndex, moduleIndex, behaviorID, ownerID);
        }

        // Set variables
        module = GetModule();
        SetStartingPosition();
        SetScale();

        //Enables sprite, animator, particles, and collider as needed
        Debug.Log($"Uses Sprite: {module.UsesSprite}, Animated: {module.Animated}, Generates Particles: {module.GeneratesParticles}");
        if (module.UsesSprite)
        {
            Debug.Log($"Sprite would be enabled here");
            EnableSprite();
        }
        if (module.Animated)
        {
            Debug.Log($"Animator would be enabled here");
            EnableAnimator();
        }
        if (module.GeneratesParticles)
        {
            Debug.Log($"Particle System would be enabled here");
            EnableParticleSystem();
        }
        if (module.UsesCollider)
        {
            gameObject.GetComponent<PolygonCollider2D>().enabled = true;
            SetCollider();
        }

        switch (module.ModuleType)
        {
            case SpellData.ModuleTypes.PlayerAttached:
                attachmentTime = module.AttachmentTime;
                characterControls = transform.parent.GetComponent<CharacterControls>();
                if (module.PushesPlayer)
                {
                    movementDirection = characterControls.movementAction.ReadValue<Vector2>().normalized;
                    // If the player is stationary, send down
                    if (movementDirection == Vector2.zero)
                    {
                        movementDirection = new Vector2(0, -1);
                    }
                }
                break;
        }

        void SetStartingPosition()
        {
            Debug.Log($"Setting starting position!");
            switch (module.ProjectileSpawningArea)
            {
                case SpellData.SpawningAreas.Point:
                    transform.position = GameSettings.Used.Characters[ownerID].CharacterSpellManager.transform.position;
                    break;
                case SpellData.SpawningAreas.AdjacentCorners:

                    SpellManager spellManager = GameSettings.Used.Characters[ownerID].CharacterSpellManager;
                    Quaternion alignment = spellManager.transform.rotation * Quaternion.Euler(0, 0, -90);
                    // Sets the position and rotation
                    transform.SetPositionAndRotation(AdjacentCornersPos(spellManager), alignment);
                    break;
                default:
                    Debug.LogWarning("Not yet implemented spawning area!");
                    break;
            }

            Vector2 AdjacentCornersPos(SpellManager spellManager)
            {
                CursorLogic cursorLogic = spellManager.GetComponent<CursorLogic>();
                int cursorWall = cursorLogic.GetCurrentWall();
                Vector2[] corners = cursorLogic.GetCurrentSquareCorners();

                // Points to instantiate at
                Vector2[] spawnPoints = new Vector2[]
                {
                        corners[cursorWall],
                        corners[(cursorWall + 1) % 4]
                };
                return spawnPoints[behaviorID];
            }
        }
        void SetScale()
        {
            transform.localScale = new Vector3(module.InstantiationScale, module.InstantiationScale, 1);
        }
        void EnableSprite()
        {
            Debug.Log($"Enabling sprite.");
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.enabled = module.UsesSprite;
            spriteRenderer.sprite = module.Sprite;

            // Set the mask layer
            string spellMaskLayer = GameSettings.Used.Characters[ownerID].OpponentCharacterInfo.CharacterAndSortingTag;
            Debug.Log($"SpellMaskLayer: {spellMaskLayer}");
            spriteRenderer.sortingLayerName = spellMaskLayer;
        }
        void EnableAnimator()
        {
            animator = gameObject.GetComponent<Animator>();
            foreach (GameObject animationPrefab in module.MultipartAnimationPrefabs)
            {
                GameObject currentAnimationPrefab = Instantiate(animationPrefab, transform);

                currentAnimationPrefab.transform.SetPositionAndRotation(transform.position, transform.rotation);
                currentAnimationPrefab.transform.localScale = new Vector2(module.AnimationScaleMultiplier, module.AnimationScaleMultiplier);
                // Animator does not work with changed name, so this line resets the name.
                currentAnimationPrefab.name = animationPrefab.name;

                // Set the mask layer
                string spellMaskLayer = GameSettings.Used.Characters[ownerID].OpponentCharacterInfo.CharacterAndSortingTag;
                currentAnimationPrefab.GetComponent<SpriteRenderer>().sortingLayerName = spellMaskLayer;
            }

            // Enables the animator if Animated is set to true
            animator.enabled = module.Animated;

            // Sets the animation
            animator.runtimeAnimatorController = module.AnimatorController;
        }
        void EnableParticleSystem()
        {
            Debug.Log($"Enabling module particle system");
            GameObject particleObject = Instantiate(module.ParticleSystemPrefab, transform);
            particleObject.transform.localPosition = new Vector3(0, 0, module.ParticleSystemZ);
        }
        void SetCollider()
        {
            gameObject.GetComponent<PolygonCollider2D>().points = module.ColliderPath;
        }
    }

    private SpellData.Module GetModule()
    {
        SpellSetInfo set = GameSettings.Used.SpellSets[spellIndex];
        SpellData spell = set.spellsInSet[spellIndex];
        return spell.UsedModules[moduleIndex];
    }

    [ClientRpc]
    private void ModuleDataClientRpc(byte serverSetIndex, byte serverSpellIndex, byte serverModuleIndex, byte serverBehaviorID, byte serverOwnerID)
    {
        if (IsHost)
        {
            Debug.Log($"Skipping recieving module data from server since this client is a host.");
            return;
        }
        setIndex = serverSetIndex;
        spellIndex = serverSpellIndex;
        moduleIndex = serverModuleIndex;
        behaviorID = serverBehaviorID;
        ownerID = serverOwnerID;
        Debug.Log($"This client recieved data from the server!\n(data was - setIndex: {setIndex}, spellIndex: {spellIndex}, moduleIndex: {moduleIndex}, behaviorID: {behaviorID}, ownerID: {ownerID})");
    }

    private void FixedUpdate()
    {
        if (IsServer) ServerPositionTick();

        switch (module.ModuleType)
        {
            case SpellData.ModuleTypes.Projectile:
                MoveSpell();
                break;
            case SpellData.ModuleTypes.PlayerAttached:
                PlayerAttachedUpdate();
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
                Destroy(gameObject);
            }
        }
    }
    private void PlayerAttachedUpdate()
    {
        TryPushPlayer();
        TryAffectPlayerMovement();

        // Attatchment Time
        attachmentTime -= Time.fixedDeltaTime;
        if (attachmentTime <= 0)
        {
            Destroy(gameObject);
        }

        #region PlayerAttachedLocalMethods
        void TryPushPlayer()
        {
            if (module.PushesPlayer)
            {
                characterControls.tempPush += module.PlayerPushSpeed * movementDirection;
                TryAnglingPush();
            }
            if (module.SpriteFacingPush)
            {
                transform.rotation = Quaternion.Euler(0, 0, 180 + GetAngle(movementDirection));
            }
            //transform.right = targetedCharacter.transform.position
        }
        void TryAffectPlayerMovement()
        {
            if (module.AffectsPlayerMovement)
            {
                characterControls.tempMovementMod = module.PlayerMovementMod;
            }
        }
        void TryAnglingPush()
        {
            if (module.AngleAfterStart)
            {
                Vector2 inputVector = characterControls.movementAction.ReadValue<Vector2>();
                float movingDirection = GetAngle(movementDirection);
                float inputDirection = GetAngle(inputVector);
                if (inputVector == Vector2.zero)
                    return;
                float movementCap = module.AngleChangeSpeed * Time.fixedDeltaTime;
                float rotationAngle = Mathf.MoveTowardsAngle(movingDirection, inputDirection, movementCap);
                movementDirection = Quaternion.Euler(0, 0, rotationAngle) * Vector2.up;
            }

        }
        float GetAngle(Vector2 vector)
        {
            // Returns angle from top, counterclockwise
            return Vector2.SignedAngle(Vector2.up, vector);
        }
        #endregion
    }
    private void ServerPositionTick()
    {
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
        if (module.TargetingType == SpellData.TargetTypes.Character)
        {
            if (targetedCharacter == null)
            {
                Debug.LogWarning("Targeted character assigned as null");
            }
            else
            {
                transform.right = targetedCharacter.transform.position - transform.position; // Point towards character
            }
        }
        else if (module.TargetingType == SpellData.TargetTypes.NotApplicable)
        {
            //Do nothing
            return;
        }
        else
        {
            Debug.LogWarning("Targeting type is not yet implemented.");
        }
    }
    private void MoveSpell()
    {
        // Move the spell
        if (module.MovementType == SpellData.MovementTypes.Linear)
        {
            transform.position += module.MovementSpeed * Time.fixedDeltaTime * transform.right;
            distanceMoved += module.MovementSpeed * Time.fixedDeltaTime;
        }
        else if (module.MovementType == SpellData.MovementTypes.Wall)
        {
            switch (behaviorID)
            {
                case 0:
                    transform.position += transform.rotation * Vector3.up * Time.fixedDeltaTime * module.MovementSpeed;
                    break;
                case 1:
                    transform.position += transform.rotation * Vector3.down * Time.fixedDeltaTime * module.MovementSpeed;
                    break;
                default:
                    Debug.LogWarning($"BehaviorID {behaviorID} should not be possible in this situation.");
                    break;
            }
            distanceMoved += Time.fixedDeltaTime * module.MovementSpeed;
        }

        // Scaling
        if (module.ScalesOverTime)
            UpdateScaling();
    }
    private void UpdateScaling()
    {
        float distanceToMove = GameSettings.Used.BattleSquareWidth / 2;
        float distanceForScaling = distanceToMove * module.ScalingStartPercent;

        if (distanceMoved >= distanceForScaling)
        {
            float currentScale = Scaling(distanceToMove, module.ScalingStartPercent, distanceMoved, module.MaxScaleMultiplier - 1);
            transform.localScale = new Vector3(module.InstantiationScale * currentScale, module.InstantiationScale * currentScale, 1);
        }

        if (distanceMoved >= distanceToMove && module.DestroyOnScalingCompleted)
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
        if (MultiplayerManager.IsOnline == false || (IsClient && !IsHost))
        {
            Destroy(gameObject);
        }
        else if (IsServer)
        {
            Debug.Log($"Despawning projectile.");
            NetworkObject.Despawn(gameObject);
        }
    }
}