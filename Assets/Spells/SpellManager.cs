using System;
using Unity.Netcode;
using UnityEngine;

public class SpellManager : NetworkBehaviour
{
    [HideInInspector] public CharacterManager characterPartsManager;
    private SpellbookLogic spellbookLogicScript;
    public readonly NetworkVariable<byte> networkCharacterIndex = new();

    private void Start()
    {
        spellbookLogicScript = characterPartsManager.SpellbookObject.GetComponent<SpellbookLogic>();
    }

    public static SpellData GetSpellData(byte setIndex, byte spellIndex)
    {
        SpellSetInfo set = GameSettings.Used.SpellSets[setIndex];
        if(spellIndex >= set.spellsInSet.Length)
        {
            Debug.LogWarning($"Set {set.name} does not have a spell at index {spellIndex}.");
        }
        return set.spellsInSet[spellIndex];
    }
    public static SpellData GetSpellData(CharacterInfo.Spellbook currentBook, byte slot)
    {
        return GetSpellData(currentBook.SetIndexes[slot], currentBook.SpellIndexes[slot]);
    }

    public void AttemptSpell(byte slot)
    {
        // Check slot validity
        if (GetSpellData(characterInfo.CurrentBook, slot) == null)
        {
            Debug.Log($"No spell in slot {slot}");
            return;
        }

        // Gets the spell in the slot
        SpellData spellData = GetSpellData(characterInfo.CurrentBook, slot);

        // Check cooldown and mana
        bool canCastSpell = CooldownAndManaAvailable(spellData, slot, false);
        if (canCastSpell == false)
            return;

        // Debug.Log($"Starting instantation of spell in slot {slot}");
        for (byte i = 0; i < spellData.UsedModules.Length; i++)
        {
            SpellData.Module module = spellData.UsedModules[i];

            SpellModuleBehavior[] moduleBehaviors = InstantiateModule(module);

            for (byte j = 0; j < moduleBehaviors.Length; j++)
            {
                SpellModuleBehavior behavior = moduleBehaviors[j]; 
                behavior.setIndex = characterInfo.CurrentBook.SetIndexes[slot];
                behavior.spellIndex = characterInfo.CurrentBook.SpellIndexes[slot];
                Debug.Log($"SetIndex: {characterInfo.CurrentBook.SetIndexes[slot]}, SpellIndex: {characterInfo.CurrentBook.SpellIndexes[slot]}");
                behavior.moduleIndex = i;
                behavior.behaviorId = j;
                behavior.ownerId = (byte)Array.IndexOf(GameSettings.Used.Characters, characterInfo);
                
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
        if (characterInfo.SpellbookLogicScript.spellCooldowns[slot] > 0)
        {
            Debug.Log("Spell on cooldown.");
            return false;
        }
        else if (spellData.ManaCost > characterInfo.Stats.CurrentMana)
        {
            Debug.Log("Not enough mana.");
            return false;
        }
        else
        {
            if ((modifyOnlyAsClient && IsServer) == false)
            {
                Debug.Log("Deducting mana!");
                characterInfo.SpellbookLogicScript.spellCooldowns[slot] = spellData.SpellCooldown;
                characterInfo.Stats.CurrentMana -= spellData.ManaCost;
                if (!IsServer)
                {
                    characterInfo.Stats.ManaAwaiting += spellData.ManaCost;
                    characterInfo.Stats.ManaAwaitingCountdown = GameSettings.Used.ManaAwaitingTimeLimit;
                }
            }
            
            return true;
        }
    }

    [ServerRpc]
    public void AttemptSpellServerRpc(byte slotIndex)
    {
        Debug.Log($"ServerRpc recieved, attempting spell server-side");
        AttemptSpell(slotIndex);
    }

    private SpellModuleBehavior[] InstantiateModule(SpellData.Module module)
    {
        SpellModuleBehavior[] spellBehaviors = new SpellModuleBehavior[module.InstantiationQuantity];

        for (var i = 0; i < module.InstantiationQuantity; i++)
        {
            spellBehaviors[i] = Instantiate(module.Prefab).GetComponent<SpellModuleBehavior>();
        }
        return spellBehaviors;
    }
}