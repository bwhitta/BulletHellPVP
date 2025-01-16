using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class SpellSpawner : NetworkBehaviour
{
    // very likely that I'll want to split up this script
    [SerializeField] private SpellbookLogic spellbookLogic;
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private CharacterStats characterStats;
    [SerializeField] private GameObject modulePrefab;
    [SerializeField] private string castingActionName;

    private void Start()
    {
        InputActionMap controlsMap = ControlsManager.GetActionMap(characterManager.InputMapName);
        InputAction castingAction = controlsMap.FindAction(castingActionName, true);
        castingAction.Enable();
        castingAction.performed += context => CastingInputPerformed((byte)(castingAction.ReadValue<float>() - 1f));
    }
    private void CastingInputPerformed(byte spellbookSlotIndex)
    {
        if (MultiplayerManager.IsOnline)
        {
            if (!IsOwner)
            {
                Debug.Log($"Can't cast spells, not owner.");
                return;
            }

            // potentially first check max mana here, and then deduct mana from here for the client-side if the player isn't the host
            SpellData spellData = spellbookLogic.CurrentBook.SpellInSlot(spellbookSlotIndex);
            bool canCastSpell = CooldownAndManaAvailable(spellData, spellbookSlotIndex, true);

            if (canCastSpell)
            {
                AttemptSpellServerRpc(spellbookSlotIndex);
            }
            else
            {
                Debug.Log("Skipped casting spell - there is not enough mana or the spell is on cooldown.");
            }
        }
        else
        {
            AttemptSpell(spellbookSlotIndex);
        }
    }
    public void AttemptSpell(byte slot)
    {
        // Check slot validity
        if (spellbookLogic.CurrentBook.SpellInSlot(slot) == null)
        {
            Debug.Log($"No spell in slot {slot}");
            return;
        }

        // Gets the spell in the slot
        SpellData spellData = spellbookLogic.CurrentBook.SpellInSlot(slot);

        // Check cooldown and mana
        bool canCastSpell = CooldownAndManaAvailable(spellData, slot, false);
        if (canCastSpell == false)
            return;
        
        for (byte i = 0; i < spellData.UsedModules.Length; i++)
        {
            SpellData.Module module = spellData.UsedModules[i];

            SpellModuleBehavior[] moduleBehaviors = InstantiateModule(module);

            for (byte j = 0; j < moduleBehaviors.Length; j++)
            {
                SpellModuleBehavior behavior = moduleBehaviors[j]; 
                behavior.setIndex = spellbookLogic.CurrentBook.SetIndexes[slot];
                behavior.spellIndex = spellbookLogic.CurrentBook.SpellIndexes[slot];
                behavior.moduleIndex = i;
                behavior.behaviorId = j;
                behavior.ownerId = (byte)OwnerClientId;
                
                if (IsServer)
                {
                    NetworkObject moduleObject = behavior.gameObject.GetComponent<NetworkObject>();
                    moduleObject.Spawn(true);
                    Debug.Log($"Spawned behavior {behavior} online");
                }
            }
        }
    }
    public bool CooldownAndManaAvailable(SpellData spellData, byte slot, bool modifyOnlyAsClient)   
    {
        if (spellbookLogic.spellCooldowns[slot] > 0)
        {
            Debug.Log("Spell on cooldown.");
            return false;
        }
        else if (spellData.ManaCost > characterStats.CurrentMana)
        {
            Debug.Log("Not enough mana.");
            return false;
        }
        else
        {
            if ((modifyOnlyAsClient && IsServer) == false)
            {
                Debug.Log("Deducting mana!");
                spellbookLogic.spellCooldowns[slot] = spellData.SpellCooldown;
                characterStats.CurrentMana -= spellData.ManaCost;
                if (!IsServer)
                {
                    characterStats.ManaAwaiting += spellData.ManaCost;
                    characterStats.ManaAwaitingCountdown = GameSettings.Used.ManaAwaitingTimeLimit;
                }
            }
            
            return true;
        }
    }
    private SpellModuleBehavior[] InstantiateModule(SpellData.Module module)
    {
        SpellModuleBehavior[] spellBehaviors = new SpellModuleBehavior[module.InstantiationQuantity];

        for (var i = 0; i < module.InstantiationQuantity; i++)
        {
            spellBehaviors[i] = Instantiate(modulePrefab).GetComponent<SpellModuleBehavior>();
        }
        return spellBehaviors;
    }

    [ServerRpc]
    public void AttemptSpellServerRpc(byte slotIndex)
    {
        Debug.Log($"ServerRpc recieved, attempting spell in slot {slotIndex} server-side");
        AttemptSpell(slotIndex);
    }
}