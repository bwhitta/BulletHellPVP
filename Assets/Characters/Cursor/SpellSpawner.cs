using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpellSpawner : NetworkBehaviour
{
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private SpellbookLogic spellbookLogic;
    [SerializeField] private CharacterStats characterStats;
    [SerializeField] private GameObject modulePrefab;
    [SerializeField] private CursorMovement cursorMovement;

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
            if (!IsOwner)
            {
                Debug.Log($"Can't cast spell, not owner.");
                return;
            }

            SpellData spellData = spellbookLogic.CurrentBook.SpellInfos[slot].Spell;

            // Checks mana and cooldown on client first before sending attempt to server
            if (CooldownAndManaAvailable(spellData, slot))
            {
                Debug.Log($"Asking server to attempt spell, frame {Time.frameCount}");
                
                if (!IsServer)
                {
                    // if the spell gets cancelled just take it directly off cooldown
                    spellbookLogic.SpellCooldowns[slot] = spellData.SpellCooldown;
                    // marks down mana that has been deducted but is waiting on actually being cast from the server
                    characterStats.ManaAwaiting += spellData.ManaCost;
                    characterStats.CurrentMana -= spellData.ManaCost;
                }

                AttemptSpellServerRpc(slot);
            }
        }
        else
        {
            AttemptSpell(slot);
        }
    }
    private void AttemptSpell(byte slot)
    {
        Debug.Log($"Attempting spell");
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
        
        // Tell the non-host client to deduct their mana as well
        if (MultiplayerManager.IsOnline)
        {
            // Tell the non-host client that a spell has been cast
            DeductManaRpc(spellInfo);
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
        Spell spellObject = Instantiate(modulePrefab).GetComponent<Spell>();

        // Starting location
        Vector2 startingPosition = module.StartingPosition.GetPosition(moduleObjectIndex, cursorMovement.Location, targetId);
        Quaternion startingRotation = module.StartingRotation.GetRotation(startingPosition, cursorMovement.Location, targetId);
        spellObject.transform.SetLocalPositionAndRotation(startingPosition, startingRotation);

        // Send info
        spellObject.SetModuleData(moduleInfo, moduleObjectIndex, targetId);

        // Spawn online
        if (MultiplayerManager.IsOnline)
        {
            NetworkObject spellNetworkObject = spellObject.GetComponent<NetworkObject>();
            spellNetworkObject.Spawn(true);
            spellObject.ModuleDataClientRpc(moduleInfo, moduleObjectIndex, targetId);
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
        Debug.Log($"ServerRpc recieved, attempting spell in slot {slot} server-side. frame {Time.frameCount}");
        AttemptSpell(slot);
    }
    // delete if I end up not using ManaAwaiting
    [Rpc(SendTo.NotServer)]
    private void DeductManaRpc(SpellData.SpellInfo spellInfo)
    {
        Debug.Log($"Deducting cooldown and mana! frame {Time.frameCount}");
        // Remove spent mana from ManaAwaiting if this client was responsible for casting the spell
        if (IsOwner)
        {
            characterStats.ManaAwaiting -= spellInfo.Spell.ManaCost;
        }
        // Deduct the mana
        characterStats.CurrentMana -= spellInfo.Spell.ManaCost;
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