using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpellSpawner : NetworkBehaviour
{
    // Fields
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private SpellbookLogic spellbookLogic;
    [SerializeField] private CharacterStats characterStats;
    [SerializeField] private GameObject modulePrefab;
    [SerializeField] private CursorMovement cursorMovement;

    // Methods
    private void Start()
    {
        // Set up controls
        InputActionMap controlsMap = ControlsManager.GetActionMap(characterManager.InputMapName);
        InputAction castingAction = controlsMap.FindAction(GameSettings.InputNames.CastingAction, true);
        castingAction.Enable();
        castingAction.performed += context => CastingInputPerformed((byte)(castingAction.ReadValue<float>() - 1f));
    }
    
    private void CastingInputPerformed(byte slot)
    {
        if (MultiplayerManager.IsOnline)
        {
            SpellData spellData = spellbookLogic.CurrentBook.SpellInfos[slot].Spell;

            // Make sure this client can actually cast spells
            if (!IsOwner)
            {
                Debug.Log($"Can't cast spell, not owner.");
                return;
            }
            else if (!CooldownAndManaAvailable(spellData, slot))
            {
                Debug.Log($"Skipped casting spell - client does not have enough mana or the spell is on cooldown.");
                return;
            }

            // Put the spell on cooldown and mark down mana as awaiting being cast
            if (!IsServer)
            {
                spellbookLogic.SpellCooldowns[slot] = spellData.SpellCooldown;
                characterStats.ManaAwaiting += spellData.ManaCost;
                characterStats.CurrentMana -= spellData.ManaCost;

                // probably will want to reorganize this script a bit later on.
                Debug.Log($"predictive spawning spell");
                SpellData.SpellInfo spellInfo = spellbookLogic.CurrentBook.SpellInfos[slot];

                // Summon each module
                for (byte i = 0; i < spellInfo.Spell.UsedModules.Length; i++)
                {
                    Debug.Log($"predictive spawning object {i}");
                    SpellModule.ModuleInfo moduleInfo = new(spellInfo, i);

                    byte targetId = GetModuleTargetId(moduleInfo.Module);
                    for (byte moduleObjectIndex = 0; moduleObjectIndex < moduleInfo.Module.InstantiationQuantity; moduleObjectIndex++)
                    {
                        Debug.Log($"spawning spell object (moduleObjectIndex: {moduleObjectIndex}");
                        Vector2 startingPosition = moduleInfo.Module.StartingPosition.GetPosition(moduleObjectIndex, cursorMovement.Location, targetId);
                        Quaternion startingRotation = moduleInfo.Module.StartingRotation.GetRotation(startingPosition, cursorMovement.Location, targetId);
                        GameObject spellGameObject = SpellPredictiveSpawner.Instance.SpawnSpellObject(startingPosition, startingRotation);
                        var spellObject = spellGameObject.GetComponent<Spell>();
                        spellObject.SetModuleData(moduleInfo, moduleObjectIndex, targetId);
                    }
                }
            }
            
            AttemptSpellServerRpc(slot);
        }
        else
        {
            AttemptSpell(slot);
        }

        // this method is directly from CastSpell, probably will merge them when restructuring
        byte GetModuleTargetId(SpellModule module)
        {
            return module.SpellTarget switch
            {
                SpellModule.SpellTargets.Owner => characterManager.CharacterIndex,
                SpellModule.SpellTargets.Opponent => characterManager.OpponentCharacterIndex,
                _ => throw new ArgumentException("Invalid spell target!")
            };
        }
    }
    private void AttemptSpell(byte slot)
    {
        SpellData.SpellInfo spellInfo = spellbookLogic.CurrentBook.SpellInfos[slot];

        // Check cooldown and mana
        if (!CooldownAndManaAvailable(spellInfo.Spell, slot))
        {
            Debug.Log($"Skipped casting spell - there is not enough mana or the spell is on cooldown.");
            // If a non-host client cast the spell, tell them that it was cancelled
            if ( !IsOwnedByServer )
            {
                CancelSpellRpc(spellInfo, slot);
            }
            return;
        }

        // Deduct mana and put spell on cooldown
        characterStats.CurrentMana -= spellInfo.Spell.ManaCost;
        spellbookLogic.SpellCooldowns[slot] = spellInfo.Spell.SpellCooldown;
        
        // Tell the non-host client that a spell has been cast
        if (MultiplayerManager.IsOnline)
        {
            VerifySpellCastRpc(spellInfo, slot);
        }
        
        CastSpell(spellInfo);
    }
    
    // could merge CastSpell with AttemptSpell
    private void CastSpell(SpellData.SpellInfo spellInfo)
    {
        // Summon each module
        for (byte i = 0; i < spellInfo.Spell.UsedModules.Length; i++)
        {
            SpellModule.ModuleInfo moduleInfo = new(spellInfo, i);

            byte targetId = GetModuleTargetId(moduleInfo.Module);
            for (byte j = 0; j < moduleInfo.Module.InstantiationQuantity; j++)
            {
                CreateSpellObject(moduleInfo, targetId, j);
            }
        }

        // Local Methods
        byte GetModuleTargetId(SpellModule module)
        {
            return module.SpellTarget switch
            {
                SpellModule.SpellTargets.Owner => characterManager.CharacterIndex,
                SpellModule.SpellTargets.Opponent => characterManager.OpponentCharacterIndex,
                _ => throw new ArgumentException("Invalid spell target!")
            };
        }
    }
    private void CreateSpellObject(SpellModule.ModuleInfo moduleInfo, byte targetId, byte moduleObjectIndex)
    {
        SpellModule module = moduleInfo.Module;
        // Spell spellObject = Instantiate(modulePrefab).GetComponent<Spell>(); OLD SPAWNING CODE
        Vector2 startingPosition = module.StartingPosition.GetPosition(moduleObjectIndex, cursorMovement.Location, targetId);
        Quaternion startingRotation = module.StartingRotation.GetRotation(startingPosition, cursorMovement.Location, targetId);
        /*GameObject spellGameObject;
        if (MultiplayerManager.IsOnline)
        {
            //Debug.LogWarning($"I think I know what the problem is here - this script assumes that the owner should always be the one calling for the object to be created, and so when the server (who is not the owner of the non-host client's character) tries to spawn the spell for that client it gets all weird.");
            spellGameObject = SpellPredictiveSpawner.Instance.SpawnSpellObject(startingPosition, startingRotation); // should probably only be predictive spawned if its player attatched, not sure how possible that is.
        }
        else
        {
            spellGameObject = Instantiate(modulePrefab, startingPosition, startingRotation);
        }*/
        GameObject spellGameObject = Instantiate(modulePrefab, startingPosition, startingRotation);
        Spell spellObject = spellGameObject.GetComponent<Spell>();

        // Starting location ALL OLD SPAWNING CODE
        //Vector2 startingPosition = module.StartingPosition.GetPosition(moduleObjectIndex, cursorMovement.Location, targetId);
        //Quaternion startingRotation = module.StartingRotation.GetRotation(startingPosition, cursorMovement.Location, targetId);
        //spellObject.transform.SetLocalPositionAndRotation(startingPosition, startingRotation);

        // Send info
        spellObject.SetModuleData(moduleInfo, moduleObjectIndex, targetId);

        // Spawn online
        if (MultiplayerManager.IsOnline)
        {
            NetworkObject spellNetworkObject = spellObject.GetComponent<NetworkObject>();
            //spellNetworkObject.Spawn(true); OLD SPAWNING CODE
            spellObject.ModuleDataClientRpc(moduleInfo, moduleObjectIndex, targetId); // might not be necessary (or at least might need to be reworked) given new predictive spawning code
        }
    }
    private bool CooldownAndManaAvailable(SpellData spellData, byte spellbookSlot)
    {
        bool cooldownAvailable = spellbookLogic.SpellCooldowns[spellbookSlot] == 0;
        bool manaAvailable = spellData.ManaCost < characterStats.CurrentMana;
        return cooldownAvailable && manaAvailable;
    }

    // Networking
    [Rpc(SendTo.Server)]
    private void AttemptSpellServerRpc(byte slot)
    {
        AttemptSpell(slot);
    }
    [Rpc(SendTo.NotServer)]
    private void VerifySpellCastRpc(SpellData.SpellInfo spellInfo, byte slot)
    {
        // Remove spent mana from ManaAwaiting if this client was responsible for casting the spell
        if (IsOwner)
        {
            characterStats.ManaAwaiting -= spellInfo.Spell.ManaCost;
        }
        // Deduct the mana and put the spell on cooldown
        characterStats.CurrentMana -= spellInfo.Spell.ManaCost;
        spellbookLogic.SpellCooldowns[slot] = spellInfo.Spell.SpellCooldown;

    }
    [Rpc(SendTo.NotServer)]
    private void CancelSpellRpc(SpellData.SpellInfo spellInfo, byte slot)
    {
        Debug.LogWarning($"Spell cancelled, didn't have enough mana or spell was on cooldown on server. Refunding mana and taking spell off cooldown.");
        characterStats.CurrentMana += spellInfo.Spell.ManaCost;
        characterStats.ManaAwaiting -= spellInfo.Spell.ManaCost;
        spellbookLogic.SpellCooldowns[slot] = 0;
    }
}