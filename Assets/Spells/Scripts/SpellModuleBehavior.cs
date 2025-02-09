using Unity.Netcode;
using UnityEngine;

public class SpellModuleBehavior : NetworkBehaviour
{
    // rename this script or remove it soon

    // Fields
    /*
    public SpellData ModuleSpellData
    {
        get
        {
            SpellSetInfo set = GameSettings.Used.SpellSets[spellIndex];
            SpellData spell = set.spellsInSet[spellIndex];
            return spell;
        }
    }*/ // gonna wait and see if allowing spells to access their parent SpellData is too much
    public byte setIndex;
    public byte spellIndex;
    public byte moduleIndex;
    public byte behaviorIndex;

    // public byte ownerId; REMOVED FOR RESTRUCTURING, leave out if unnecessary

    // private float attachmentTime; REMOVED FOR RESTRUCTURING
    // public float cursorLocationOnCast; REMOVED FOR RESTRUCTURING

    private readonly float outOfBoundsDistance = 15f;

    // Properties
    public SpellData.Module Module
    {
        get
        {
            SpellSetInfo set = GameSettings.Used.SpellSets[setIndex];
            SpellData spell = set.spellsInSet[spellIndex];
            return spell.UsedModules[moduleIndex];
        }
    }

    // Methods
    private void Start()
    {
        // If local or server set cursor position on cast
        if (!MultiplayerManager.IsOnline || IsServer)
        {
            // probably should rework this anyways so that it only tracks the cursor's position on cast if it matters (if it tracks it at all)
            // cursorPositionOnCast = OwnerCharacterInfo.CursorLogicScript.location; REMOVED FOR RESTRUCTURING
        }
        
        // Send data to client
        if (IsServer)
        {
            Debug.Log($"Sending module data to clients");
            ModuleDataClientRpc(setIndex, spellIndex, moduleIndex, behaviorIndex/*, cursorLocationOnCast REMOVED FOR RESTRUCTURING*/);
        }
    }
    private void FixedUpdate()
    {

        // Delete if too far away
        CheckBounds();
        void CheckBounds()
        {
            float distanceFromCenter = Vector2.Distance(transform.position, Vector2.zero);
            if (distanceFromCenter >= outOfBoundsDistance)
            {
                Debug.Log($"Deleted - out of bounds");
                // DestroySelfNetworkSafe(); REMOVED FOR RESTRUCTURING
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
    } REMOVED FOR RESTRUCTURING */

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
    private void ModuleDataClientRpc(byte serverSetIndex, byte serverSpellIndex, byte serverModuleIndex, byte serverBehaviorIndex/*, float serverCursorPositionOnCast REMOVED FOR RESTRUCTURING*/)
    {
        if (IsHost)
        {
            return;
        }
        setIndex = serverSetIndex;
        spellIndex = serverSpellIndex;
        moduleIndex = serverModuleIndex;
        behaviorIndex = serverBehaviorIndex;
        // cursorLocationOnCast = serverCursorPositionOnCast; REMOVED FOR RESTRUCTURING

        // Only deduct Mana Awaiting if this is the first SpellModuleBehavior
        if (behaviorIndex == 0)
        {
            // OwnerCharacterInfo.Stats.ManaAwaiting -= ModuleSpellData.ManaCost; REMOVED FOR RESTRUCTURING, also I don't get how this even works. Is the spawned spell seriously responsible for deducting the mana?
        }
    }
}