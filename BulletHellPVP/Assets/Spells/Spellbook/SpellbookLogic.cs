using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SpellbookLogic : MonoBehaviour
{
    [SerializeField] private GameObject[] spellDisplays;
    [SerializeField] private CharacterInfo characterInfo;
    [HideInInspector] public float[] spellCooldowns;

    private void Awake()
    {
        gameObject.tag = characterInfo.CharacterTag;
    }
    private void Start()
    {
        // Turn off spellbook if character doesn't exist
        if (characterInfo.CharacterObject == null)
        {
            SpellbookToggle(false);
        }
    }
    public void SpellbookToggle(bool enable)
    {
        gameObject.SetActive(enable);
        EnableSpellControls(enable);
        if (enable)
        {
            UpdateSpellbookUI();
        }
        else
        {
            characterInfo.SpellbookLogicScript = this;
        }
    }

    // Enabling spell controls
    private InputActionMap controlsMap;
    private InputAction castingAction;
    private void EnableSpellControls(bool enable)
    {
        if (enable)
        {
            // Finds the controls
            controlsMap ??= ControlsManager.GetActionMap(characterInfo.InputMapName);
            castingAction ??= controlsMap.FindAction(characterInfo.SpellbookSelectionActionName, true);
            // Enable controls
            castingAction.Enable();
            castingAction.performed += context => CastingInputPerformed((int)castingAction.ReadValue<float>() - 1);
        }
        else
        {
            // Finds the controls
            controlsMap ??= ControlsManager.GetActionMap(characterInfo.InputMapName);
            castingAction ??= controlsMap.FindAction(characterInfo.SpellbookSelectionActionName, true);
            // Disable controls
            castingAction.Disable();
        }
    }
    private void UpdateSpellbookUI()
    {
        // Loop through and update each sprite using the data from 
        for (var i = 0; i < spellDisplays.Length; i++)
        {
            if (characterInfo.EquippedSpells.Length <= i || characterInfo.EquippedSpells[i] == null)
            {
                // Debug.Log($"No equipped spell in slot {i}, skipping render");
                spellDisplays[i].SetActive(false);
                continue;
            }
            SpriteRenderer spriteRenderer = spellDisplays[i].GetComponent<SpriteRenderer>();
            spriteRenderer.enabled = true;
            spellDisplays[i].GetComponent<SpriteRenderer>().sprite = characterInfo.EquippedSpells[i].Icon;
        }
    }


    private void CastingInputPerformed(int spellbookSlotIndex)
    {
        if (gameObject.activeSelf == false)
        {
            Debug.Log($"Casting cancelled, spellbook disabled.");
            return;
        }
        if(spellbookSlotIndex < 0)
        {
            // Debug.Log("The given spellbook slot index is negative. Good luck...");
            return;
        }

        if (characterInfo.EquippedSpells.Length <= spellbookSlotIndex)
        {
            // Debug.Log("Spell not equipped");
            return;
        }
        Debug.Log(spellbookSlotIndex);
        if (characterInfo.EquippedSpells[spellbookSlotIndex] == null)
        {
            // Debug.Log("Slot null");
            return;
        }

        characterInfo.CharacterSpellManager.AttemptSpell(spellbookSlotIndex);
    }

    private void Update()
    {
        UpdateCooldown();
    }
    private void UpdateCooldown()
    {
        // Set up cooldowns if data is invalid
        if (spellCooldowns == null || spellCooldowns.Length < characterInfo.EquippedSpells.Length)
        {
            spellCooldowns = new float[characterInfo.gameSettings.TotalSpellSlots];
        }

        // Loop through cooldowns and tick down by time.deltatime
        for (int i = 0; i < spellCooldowns.Length; i++)
        {
            if (spellCooldowns[i] > 0)
            {

                spellCooldowns[i] -= Time.deltaTime;
            }
            if (spellCooldowns[i] < 0)
            {
                spellCooldowns[i] = 0;
            }
            //Updates the cooldown UI for i with the current percent
            if (characterInfo.EquippedSpells[i] != null)
                SetCooldownUI(i, spellCooldowns[i] / characterInfo.EquippedSpells[i].SpellCooldown);
        }
    }
    private void SetCooldownUI(int index, float percentFilled)
    {
        // Gets the top bar GameObject
        GameObject bottomBar = spellDisplays[index].transform.GetChild(0).gameObject;
        GameObject topBar = bottomBar.transform.GetChild(0).gameObject;

        topBar.GetComponent<Image>().fillAmount = percentFilled;
    }


}
