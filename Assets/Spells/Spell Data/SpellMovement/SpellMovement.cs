using UnityEngine;

public abstract class SpellMovement : ScriptableObject
{
    public float movementSpeed;

    public abstract void Move(/* add parameters maybe? */);

    // private readonly float outOfBoundsDistance = 15f; REMOVED FOR RESTRUCTURING
    // private float distanceMoved; REMOVED FOR RESTRUCTURING
    // private int ticksSincePositionUpdate; REMOVED FOR RESTRUCTURING
    // private readonly NetworkVariable<Vector2> serverSidePosition = new(); REMOVED FOR RESTRUCTURING

    // Properties
    //SpellModule Module => spellInfoLogic.Module;

    // Methods
    /*private void Start()
    {
        if (MultiplayerManager.IsOnline && !IsServer) serverSidePosition.OnValueChanged += ServerPositionChanged;
    }*/
    /*private void FixedUpdate()
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
                spellInfoLogic.DestroySelfNetworkSafe();
            }
        }
    }*/

    /* REMOVED FOR RESTRUCTURING
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
    }*/

    /*private void MoveProjectileSpell()
    {
        // Move the spell
        if (Module.MovementType == SpellData.MovementTypes.Linear)
        {
            transform.position += Module.MovementSpeed * Time.fixedDeltaTime * transform.right;
            distanceMoved += Module.MovementSpeed * Time.fixedDeltaTime;
        }
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
        } REMOVED FOR RESTRUCTURING 
        // Scaling
        if (Module.ScalesOverTime)
            UpdateScaling(); REMOVED FOR RESTRUCTURING
    }*/
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

    //Online
    /*void ServerPositionChanged(Vector2 oldValue, Vector2 newValue)
    {
        transform.position = Calculations.DiscrepancyCheck(transform.position, newValue, GameSettings.Used.NetworkLocationDiscrepancyLimit);
    }*/
    /* REMOVED FOR RESTRUCTURING
    private void ServerPositionTick()
    {
        // Discrepancy checks
        ticksSincePositionUpdate++;
        if (ticksSincePositionUpdate >= GameSettings.Used.NetworkDiscrepancyCheckFrequency)
        {
            serverSidePosition.Value = transform.position;
            ticksSincePositionUpdate = 0;
        }
    }*/
}
