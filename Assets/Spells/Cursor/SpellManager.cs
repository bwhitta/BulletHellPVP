using System;
using Unity.Netcode;
using UnityEngine;

public class SpellManager : NetworkBehaviour
{
    // Note to self: probably should rename this script, since it's not really an actual manager script

    [SerializeField] private SpellbookLogic spellbookLogicScript;
    [SerializeField] private CharacterStats characterStats;
    [SerializeField] private CharacterManager characterManager; // probably want to remove this and make it so that the id of the current book is stored as part of this object or the spellbook.

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
        if (GetSpellData(characterManager.OwnedCharacterInfo.CurrentBook, slot) == null)
        {
            Debug.Log($"No spell in slot {slot}");
            return;
        }

        // Gets the spell in the slot
        SpellData spellData = GetSpellData(characterManager.OwnedCharacterInfo.CurrentBook, slot);

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
                behavior.setIndex = characterManager.OwnedCharacterInfo.CurrentBook.SetIndexes[slot];
                behavior.spellIndex = characterManager.OwnedCharacterInfo.CurrentBook.SpellIndexes[slot];
                Debug.Log($"SetIndex: {characterManager.OwnedCharacterInfo.CurrentBook.SetIndexes[slot]}, SpellIndex: {characterManager.OwnedCharacterInfo.CurrentBook.SpellIndexes[slot]}");
                behavior.moduleIndex = i;
                behavior.behaviorId = j;
                behavior.ownerId = (byte)Array.IndexOf(GameSettings.Used.Characters, characterManager.OwnedCharacterInfo);
                
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
        if (spellbookLogicScript.spellCooldowns[slot] > 0)
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
                spellbookLogicScript.spellCooldowns[slot] = spellData.SpellCooldown;
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