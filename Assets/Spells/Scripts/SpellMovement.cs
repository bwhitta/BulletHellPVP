using UnityEngine;
using Unity.Netcode;

public class SpellMovement : NetworkBehaviour
{
    // Fields
    private readonly float outOfBoundsDistance = 15f;
    private float distanceMoved;
    private int ticksSincePositionUpdate;

    private SpellInfoLogic spellModuleBehavior; // rename as soon as I rename the spellModuleBehavior script;
    
    private readonly NetworkVariable<Vector2> serverSidePosition = new();

    // Properties
    SpellModule Module => spellModuleBehavior.Module; // probably remove this property and just access the other script once I rename it
    
    // Methods
    private void Start()
    {
        /*if (!MultiplayerManager.IsOnline || IsServer)
        {
            // probably should rework this anyways so that it only tracks the cursor's position on cast if it matters (if it tracks it at all)
            cursorPositionOnCast = OwnerCharacterInfo.CursorLogicScript.location;
        } REMOVED FOR RESTRUCTURING, this also will probably have to go somewhere else */

        spellModuleBehavior = GetComponent<SpellInfoLogic>();

        transform.position = StartingPosition();
        
        // Keep server-side position synced for clients
        if (MultiplayerManager.IsOnline && !IsServer) serverSidePosition.OnValueChanged += ServerPositionUpdate;

        // Local Methods
        void ServerPositionUpdate(Vector2 oldValue, Vector2 newValue)
        {
            transform.position = Calculations.DiscrepancyCheck(transform.position, newValue, GameSettings.Used.NetworkLocationDiscrepancyLimit);
        }
    }
    private Vector2 StartingPosition()
    {
        switch (Module.ProjectileSpawningArea)
        {
            case SpellModule.SpawningArea.Point:
                // transform.position = CursorMovement.CalculateCursorTransform(cursorPositionOnCast, OwnerCharacterInfo.OpponentAreaCenter); 
                return new(); // TEMPORARY DURING RESTRUCTURING;
            case SpellModule.SpawningArea.AdjacentCorners:
                // Turns the float position of the cursor into a rotation.
                //int side = CursorMovement.SquareSideAtPosition(GameSettings.Used.BattleSquareWidth, cursorLocationOnCast); REMOVED FOR RESTRUCTURING
                // Quaternion cursorAngleOnCast = Quaternion.Euler(0, 0, (-90 * side) - 90); // no clue why I use -90 and not 90 here, but that's what I did for other parts of the code so I won't question it. REMOVED FOR RESTRUCTURING                
                // Sets the position and rotation
                // transform.SetPositionAndRotation(AdjacentCornersPos(), cursorAngleOnCast);  REMOVED FOR RESTRUCTURING, rotation should be set somewhere else
                return new(); // TEMPORARY DURING RESTRUCTURING
            default:
                Debug.LogWarning("Not yet implemented spawning area!");
                return new();
        }

        /* // Local Methods
        Vector2 AdjacentCornersPos()
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
        } REMOVED FOR RESTRUCTURING */
    }

    private void FixedUpdate()
    {
        if (IsServer) ServerPositionTick();
        
        // okay bit weird how most module types aren't moving, I'll have to look at possibly rearranging this more. maybe it's fine tho
        switch (Module.ModuleType)
        {
            case SpellModule.ModuleTypes.Projectile:
                MoveProjectileSpell();
                break;
        }

        // Delete if too far away
        CheckBounds();

        // Local Methods 
        void CheckBounds()
        {
            float distanceFromCenter = Vector2.Distance(transform.position, Vector2.zero);
            if (distanceFromCenter >= outOfBoundsDistance)
            {
                Debug.Log($"Deleted spell - out of bounds");
                spellModuleBehavior.DestroySelfNetworkSafe();
            }
        }
    }

    /*private void UpdateScaling()
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
    } REMOVED FOR RESTRUCTURING, this stuff probably doesn't really fit in SpellMovement*/

    private void MoveProjectileSpell()
    {
        // Move the spell
        /*if (Module.MovementType == SpellData.MovementTypes.Linear)
        {
            transform.position += Module.MovementSpeed * Time.fixedDeltaTime * transform.right;
            distanceMoved += Module.MovementSpeed * Time.fixedDeltaTime;
        }*/
        /*else if (Module.MovementType == SpellData.MovementTypes.Wall)
        {
            switch (behaviorIndex)
            {
                case 0:
                    transform.position += transform.rotation * Vector3.up * Time.fixedDeltaTime * Module.MovementSpeed;
                    break;
                case 1:
                    transform.position += transform.rotation * Vector3.down * Time.fixedDeltaTime * Module.MovementSpeed;
                    break;
                default:
                    Debug.LogWarning($"behaviorId {behaviorIndex} should not be possible in this situation.");
                    break;
            }
            distanceMoved += Time.fixedDeltaTime * Module.MovementSpeed;
        } REMOVED FOR RESTRUCTURING */
        // Scaling
        /*if (Module.ScalesOverTime)
            UpdateScaling(); REMOVED FOR RESTRUCTURING */
    }
    /*private void PointTowardsTarget()
    {
        // If TargetingType is CharacterStats, point towards the character
        switch (Module.TargetingType)
        {
            case SpellData.TargetTypes.Opponent:
                //GameObject opponent = OwnerCharacterInfo.OpponentCharacterInfo.CharacterObject; REMOVED FOR RESTRUCTURING
                //transform.right = opponent.transform.position - transform.position; // Point towards character REMOVED FOR RESTRUCTURING
                break;
            case SpellData.TargetTypes.NotApplicable:
                //Do nothing
                break;
            default:
                Debug.LogWarning("Targeting type is not yet implemented.");
                break;
        }
    }*/
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
}
