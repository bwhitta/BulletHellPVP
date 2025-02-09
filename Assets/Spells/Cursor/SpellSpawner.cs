using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class SpellSpawner : NetworkBehaviour
{
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private SpellbookLogic spellbookLogic;
    [SerializeField] private CharacterStats characterStats;
    [SerializeField] private GameObject modulePrefab;
    [SerializeField] private string castingActionName;

    private void Start()
    {
        // Set up controls
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

            SpellData spellData = spellbookLogic.CurrentBook.SpellInSlot(spellbookSlotIndex);
            if (CooldownAndManaAvailable(spellData, spellbookSlotIndex))
            {
                AttemptSpellServerRpc(spellbookSlotIndex);
            }
            else
            {
                Debug.Log($"Skipped casting spell - there is not enough mana or the spell is on cooldown.");
            }
        }
        else
        {
            AttemptSpell(spellbookSlotIndex);
        }
    }
    public void AttemptSpell(byte slot)
    {
        SpellData spellData = spellbookLogic.CurrentBook.SpellInSlot(slot);

        // Check cooldown and mana
        if (!CooldownAndManaAvailable(spellData, slot))
        {
            Debug.Log($"Skipped casting spell - there is not enough mana or the spell is on cooldown.");
            return;
        }

        // not entirely sure what this does. I think its for if you cast when you are the non-host client?  
        if (MultiplayerManager.IsOnline && !IsServer)
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

        // Summon each module
        for (byte i = 0; i < spellData.UsedModules.Length; i++)
        {
            SpellData.Module module = spellData.UsedModules[i];

            // Returns an array because some modules will spawn more than one object
            SpellModuleBehavior[] moduleBehaviors = InstantiateModule(module);

            for (byte j = 0; j < moduleBehaviors.Length; j++)
            {
                SpellModuleBehavior behavior = moduleBehaviors[j]; 
                behavior.setIndex = spellbookLogic.CurrentBook.SetIndexes[slot];
                behavior.spellIndex = spellbookLogic.CurrentBook.SpellIndexes[slot];
                behavior.moduleIndex = i;
                behavior.behaviorIndex = j;
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
    // modifyOnlyAsClient should be seperated out
    public bool CooldownAndManaAvailable(SpellData spellData, byte spellbookSlot)
    {
        return spellbookLogic.spellCooldowns[spellbookSlot] > 0 || spellData.ManaCost > characterStats.CurrentMana;
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