using Unity.Netcode;
using UnityEngine;

public class SpellInfoLogic : NetworkBehaviour
{
    // going to delete this class soon

    // Methods
    /*private void Start()
    {
        if (IsServer)
        {
            ModuleDataClientRpc(SetIndex, SpellIndex, ModuleIndex, ModuleObjectIndex, OwnerId, CursorLocationOnCast);
        }
    }*/

    // probably move to another script. maybe turn into a static called just DestroyNetworkSafe that takes a GameObject as input
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

    /* REMOVED FOR RESTRUCTURING
    [ClientRpc]
    private void ModuleDataClientRpc(byte serverSetIndex, byte serverSpellIndex, byte serverModuleIndex, byte serverBehaviorIndex, byte ownerId, float serverCursorPositionOnCast)
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
        CursorLocationOnCast = serverCursorPositionOnCast;

        // Only deduct Mana Awaiting if this is the first SpellModuleBehavior
        //if (BehaviorIndex == 0)
        //{
        //    OwnerCharacterInfo.Stats.ManaAwaiting -= ModuleSpellData.ManaCost;
        //} REMOVED FOR RESTRUCTURING, also I don't get how this even works. Is the spawned spell seriously responsible for deducting the mana?
    }*/
}