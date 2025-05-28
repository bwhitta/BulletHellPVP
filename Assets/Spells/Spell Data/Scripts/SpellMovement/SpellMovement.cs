using UnityEngine;

public abstract class SpellMovement : ScriptableObject
{
    // Fields
    public float MovementSpeed;
    public float RotationOffset;
    
    // Methods
    public abstract Vector2 Move(float angle, byte moduleObjectIndex);

    /*private readonly float outOfBoundsDistance = 15f;
    private float distanceMoved;*/

    /*private void FixedUpdate()
    {
        // CODE TO MOVE SPELL WAS HERE
        
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

    /*private void MoveProjectileSpell()
    {
        if (Module.ScalesOverTime)
            UpdateScaling();
    }*/
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
    }*/
}
