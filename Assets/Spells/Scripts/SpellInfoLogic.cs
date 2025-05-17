using Unity.Netcode;
using UnityEngine;

public class SpellInfoLogic : NetworkBehaviour
{
    // Fields
    [HideInInspector] public byte SetIndex;
    [HideInInspector] public byte SpellIndex;
    [HideInInspector] public byte ModuleIndex;
    [HideInInspector] public byte ModuleObjectIndex;
    [HideInInspector] public byte OwnerId;
    // public float cursorLocationOnCast; REMOVED FOR RESTRUCTURING
    
    // Properties
    public SpellModule Module
    {
        get
        {
            SpellSetInfo set = GameSettings.Used.SpellSets[SetIndex];
            SpellData spell = set.spellsInSet[SpellIndex];
            return spell.UsedModules[ModuleIndex];
        }
    }
    public byte OpponentId
    {
        get
        {
            if (OwnerId == 0) return 1;
            else return 0;
        }
    }

    // Methods
    private void Start()
    {
        if (IsServer)
        {
            ModuleDataClientRpc(SetIndex, SpellIndex, ModuleIndex, ModuleObjectIndex, OwnerId/*, cursorLocationOnCast REMOVED FOR RESTRUCTURING*/);
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
    private void ModuleDataClientRpc(byte serverSetIndex, byte serverSpellIndex, byte serverModuleIndex, byte serverBehaviorIndex, byte ownerId/*, float serverCursorPositionOnCast REMOVED FOR RESTRUCTURING*/)
    {
        if (IsHost)
        {
            return;
        }
        SetIndex = serverSetIndex;
        SpellIndex = serverSpellIndex;
        ModuleIndex = serverModuleIndex;
        ModuleObjectIndex = serverBehaviorIndex;
        OwnerId = ownerId;
        // cursorLocationOnCast = serverCursorPositionOnCast; REMOVED FOR RESTRUCTURING

        // Only deduct Mana Awaiting if this is the first SpellModuleBehavior
        /*if (BehaviorIndex == 0)
        {
            OwnerCharacterInfo.Stats.ManaAwaiting -= ModuleSpellData.ManaCost;
        } REMOVED FOR RESTRUCTURING, also I don't get how this even works. Is the spawned spell seriously responsible for deducting the mana? */
    }
}