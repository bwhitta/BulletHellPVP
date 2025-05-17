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

            // Checks mana and cooldown on client first before sending attempt to server
            if (CooldownAndManaAvailable(spellData, spellbookSlotIndex))
            {
                AttemptSpellServerRpc(spellbookSlotIndex);
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
            spellbookLogic.SpellCooldowns[slot] = spellData.SpellCooldown;
            characterStats.CurrentMana -= spellData.ManaCost;
            
            characterStats.ManaAwaiting += spellData.ManaCost;
            characterStats.ManaAwaitingCountdown = GameSettings.Used.ManaAwaitingTimeLimit;
        }

        // Summon each module
        for (byte i = 0; i < spellData.UsedModules.Length; i++)
        {
            InstantiateModule(spellData.UsedModules[i], slot, i);
        }
    }
    private void InstantiateModule(SpellModule module, byte slot, byte moduleIndex)
    {
        SpellInfoLogic[] spellObjects = new SpellInfoLogic[module.InstantiationQuantity];

        for (byte i = 0; i < module.InstantiationQuantity; i++)
        {
            SpellInfoLogic spellObject = Instantiate(modulePrefab).GetComponent<SpellInfoLogic>();
            
            // Give any necessary info to the spell object
            spellObject.SetIndex = spellbookLogic.CurrentBook.SetIndexes[slot];
            spellObject.SpellIndex = spellbookLogic.CurrentBook.SpellIndexes[slot];
            spellObject.ModuleIndex = moduleIndex;
            spellObject.ModuleObjectIndex = i;
            spellObject.OwnerId = (byte)OwnerClientId;
            spellObjects[i] = spellObject;

            if (IsServer)
            {
                NetworkObject spellNetworkObject = spellObject.GetComponent<NetworkObject>();
                spellNetworkObject.Spawn(true);
                Debug.Log($"Spawned behavior {spellObjects} online");
            }
        }
    }
    public bool CooldownAndManaAvailable(SpellData spellData, byte spellbookSlot)
    {
        Debug.Log($"spellData: {spellData.name}, spellbookSlot: {spellbookSlot}, spellData.ManaCost: {spellData.ManaCost}, spellbookLogic.spellCooldowns[spellbookSlot]: {spellbookLogic.SpellCooldowns[spellbookSlot]}");
        bool cooldownAvailable = spellbookLogic.SpellCooldowns[spellbookSlot] == 0;
        bool manaAvailable = spellData.ManaCost < characterStats.CurrentMana;
        return cooldownAvailable && manaAvailable;
    }

    [ServerRpc]
    public void AttemptSpellServerRpc(byte slotIndex)
    {
        Debug.Log($"ServerRpc recieved, attempting spell in slot {slotIndex} server-side");
        AttemptSpell(slotIndex);
    }
}