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
    [SerializeField] private GameObject spellPrefab;
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
            if (!IsOwner)
            {
                Debug.Log($"Can't cast spell, not owner.");
                return;
            }
        }
        AttemptSpell(slot);
    }
    private void AttemptSpell(byte slot)
    {
        SpellData.SpellInfo spellInfo = spellbookLogic.CurrentBook.SpellInfos[slot];
        SpellData spell = spellInfo.Spell;

        // Check cooldown and mana
        if (!CooldownAndManaAvailable(spell, slot))
        {
            Debug.Log($"Skipped casting spell - there is not enough mana or the spell is on cooldown.");
            // If a non-host client cast the spell, tell them that it was cancelled
            if ( MultiplayerManager.IsOnline && IsServer && !IsOwnedByServer)
            {
                CancelSpellRpc(spellInfo, slot);
            }
            return;
        }

        // Deduct mana and put spell on cooldown
        characterStats.CurrentMana -= spell.ManaCost;
        spellbookLogic.SpellCooldowns[slot] = spell.SpellCooldown;
        
        if (MultiplayerManager.IsOnline)
        {
            if (IsServer)
            {
                // Tell the non-host client that a spell has been cast
                VerifySpellCastRpc(spellInfo, slot);
            }
            else
            {
                // Tell server to try casting the spell too
                Debug.Log($"attempting spell as non-server, sending serverRpc");
                AttemptSpellServerRpc(slot);

                // Mark down mana as awaiting being cast
                characterStats.ManaAwaiting += spell.ManaCost;
            }
        }
        

        CastSpell(spellInfo);
    }
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
        Vector2 startingPosition = module.StartingPosition.GetPosition(moduleObjectIndex, cursorMovement.Location, targetId);
        Quaternion startingRotation = module.StartingRotation.GetRotation(startingPosition, cursorMovement.Location, targetId);


        GameObject spellGameObject = null;
        if (MultiplayerManager.IsOnline)
        {
            bool canPredictiveSpawn = !(IsServer && IsOwner);
            bool usesPredictiveSpawning = canPredictiveSpawn && module.PlayerAttached && (module.SpellTarget == SpellModule.SpellTargets.Owner);
            
            if (usesPredictiveSpawning)
            {
                if (IsClient && !IsServer)
                {
                    spellGameObject = SpellPredictiveSpawner.Instance.ClientSpawnSpellObject(startingPosition, startingRotation);
                }
                else if (IsServer && !IsOwner)
                {
                    spellGameObject = SpellPredictiveSpawner.Instance.ServerSpawnSpellObject(OwnerClientId, startingPosition, startingRotation);
                }
                else
                {
                    Debug.Log($"predictive spawning shouldn't be possible in this state");
                }
            }
            else if (IsServer)
            {
                spellGameObject = Instantiate(spellPrefab, startingPosition, startingRotation);
                NetworkObject spellNetworkObject = spellGameObject.GetComponent<NetworkObject>();
                spellNetworkObject.Spawn(true);
                spellGameObject.GetComponent<Spell>().ModuleDataClientRpc(moduleInfo, moduleObjectIndex, targetId);
            }
        }
        else
        {
            spellGameObject = Instantiate(spellPrefab, startingPosition, startingRotation);
        }

        Spell spellObject = spellGameObject.GetComponent<Spell>();
        spellObject.SetModuleData(moduleInfo, moduleObjectIndex, targetId);
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
        // wait why does this deduct the mana upon confirmation even though it was already deducted before? disabling this for now.
        /*characterStats.CurrentMana -= spellInfo.Spell.ManaCost;
        spellbookLogic.SpellCooldowns[slot] = spellInfo.Spell.SpellCooldown;*/
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