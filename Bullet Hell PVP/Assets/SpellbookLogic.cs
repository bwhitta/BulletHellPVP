using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static ControlsManager;

public class SpellbookLogic : MonoBehaviour
{
    [SerializeField] private GameObject spellManagerObject;
    [SerializeField] private GameObject[] spellDisplays;
    [SerializeField] private string castingActionName;

    private SpellManager spellManager;
    private GameControls controls;

    private bool componentsSet = false;

    private void Start()
    {
        SetComponents();
        UpdateSpellbookUI();
        //Spell controls enabled from SpellManager script
    }
    private void SetComponents()
    {
        if (componentsSet) {
            Debug.Log("Skipping SetComponents, already set.");
            return;
        }
        controls = GameControlsMaster.GameControls;
        spellManager = spellManagerObject.GetComponent<SpellManager>();

        componentsSet = true;
    }
    private void UpdateSpellbookUI()
    {
        if (spellDisplays.Length < spellManager.equippedSpellNames.Length)
        {
            Debug.LogWarning("Too many equipped spells! Final spells will not be rendered.");
        }

        // Loop through and update each sprite using the data from 
        for (var i = 0; i < spellDisplays.Length; i++)
        {
            if (spellManager.equippedSpellNames.Length <= i) {
                // Debug.Log("No equipped spell in slot, skipping render");
                spellDisplays[i].gameObject.SetActive(false);
                continue;
            }
            spellDisplays[i].GetComponent<SpriteRenderer>().enabled = true;

            spellDisplays[i].GetComponent<SpriteRenderer>().sprite = spellManager.equippedSpellData[i].SpellbookSprite;
            // Debug.Log($"UI {i} updated");
        }
        
    }

    public void UpdateCooldownUI(int index, float percentFilled)
    {
        // Gets the top bar GameObject
        GameObject bottomBar = spellDisplays[index].transform.GetChild(0).gameObject;
        GameObject topBar = bottomBar.transform.GetChild(0).gameObject;

        topBar.GetComponent<Image>().fillAmount = percentFilled;
    }

    public void EnableSpellControls()
    {
        if(controls == null)
        {
            SetComponents();
        }
        InputAction castingAction = controls.FindAction(castingActionName);

        castingAction.Enable();
        castingAction.performed += context => CastingInputPerformed((int)castingAction.ReadValue<float>() - 1);
    }

    private void CastingInputPerformed(int spellbookSlotIndex)
    {
        // Debug.Log($"Equipped spell {spellbookSlotIndex+1} cast.");
        if (spellManager.equippedSpellNames.Length <= spellbookSlotIndex)
        {
            Debug.LogWarning("Error - not enough equipped spells.");
            return;
        }

        spellManager.CastSpell(spellManager.equippedSpellNames[spellbookSlotIndex]);
    }
}
