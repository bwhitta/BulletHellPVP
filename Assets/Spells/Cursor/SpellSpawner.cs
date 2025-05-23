using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using System.Reflection;
using System;

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

    private void CastingInputPerformed(byte spellbookSlot)
    {
        if (MultiplayerManager.IsOnline)
        {
            if (!IsOwner)
            {
                Debug.Log($"Can't cast spell, not owner.");
                return;
            }

            SpellData spellData = spellbookLogic.CurrentBook.SpellInSlot(spellbookSlot);

            // Checks mana and cooldown on client first before sending attempt to server
            if (CooldownAndManaAvailable(spellData, spellbookSlot))
            {
                Debug.Log($"should attempt spell here, deleteme.");
                // AttemptSpellServerRpc(spellbookSlotIndex);
            }
        }
        else
        {
            AttemptSpell(spellbookSlot);
        }
    }
    public void AttemptSpell(byte spellbookSlot)
    {
        SpellData spellData = spellbookLogic.CurrentBook.SpellInSlot(spellbookSlot);
        
        // Check cooldown and mana
        if (!CooldownAndManaAvailable(spellData, spellbookSlot))
        {
            Debug.Log($"Skipped casting spell - there is not enough mana or the spell is on cooldown.");
            return;
        }

        // not entirely sure what this does. I think its for if you cast when you are the non-host client?
        /* REMOVED FOR RESTRUCTURING
        if (MultiplayerManager.IsOnline && !IsServer)
        {
            Debug.Log("Deducting mana!");
            spellbookLogic.SpellCooldowns[slot] = spellData.SpellCooldown;
            characterStats.CurrentMana -= spellData.ManaCost;

            characterStats.ManaAwaiting += spellData.ManaCost;
            characterStats.ManaAwaitingCountdown = GameSettings.Used.ManaAwaitingTimeLimit;
        } */
        
        CastSpell(spellData);
    }
    public void CastSpell(SpellData spellData)
    {
        // Summon each module
        foreach (SpellModule module in spellData.UsedModules)
        {
            byte spellTargetId = GetModuleTargetId(module);
            ExecuteModule(module, spellTargetId);
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
    public void ExecuteModule(SpellModule module, byte targetId)
    {
        for (byte i = 0; i < module.InstantiationQuantity; i++)
        {
            Spell spellObject = Instantiate(modulePrefab).GetComponent<Spell>();
            
            // Give any necessary info to the spell object
            spellObject.Module = module;
            spellObject.ModuleObjectIndex = i;
            spellObject.TargetId = targetId;

            Vector2 startingPosition = module.StartingPosition.GetPosition(i, cursorMovement.location, targetId);
            Quaternion startingRotation = module.StartingRotation.GetRotation(startingPosition, cursorMovement.location, targetId);
            spellObject.transform.SetPositionAndRotation(startingPosition, startingRotation);

            /* REMOVED FOR RESTRUCTURING
            if (IsServer)
            {
                NetworkObject spellNetworkObject = spellObject.GetComponent<NetworkObject>();
                spellNetworkObject.Spawn(true);
            }*/
        }
    }
    

    // could remove spellData as a paremeter and have this method find the spellData itself
    public bool CooldownAndManaAvailable(SpellData spellData, byte spellbookSlot)
    {
        bool cooldownAvailable = spellbookLogic.SpellCooldowns[spellbookSlot] == 0;
        bool manaAvailable = spellData.ManaCost < characterStats.CurrentMana;
        return cooldownAvailable && manaAvailable;
    }

    /*[ServerRpc]
    public void AttemptSpellServerRpc(byte slotIndex)
    {
        Debug.Log($"ServerRpc recieved, attempting spell in slot {slotIndex} server-side");
        AttemptSpell(slotIndex);
    }*/
}