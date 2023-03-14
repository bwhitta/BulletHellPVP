using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SpellbookLogic : MonoBehaviour
{
    [SerializeField] private GameObject spellManagerObject;
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

    private void CastingInputPerformed(int spellbookSlotIndex)
    {
        if (gameObject.activeSelf == false)
        {
            Debug.Log($"Casting cancelled, spellbook disabled.");
            return;
        }
        if (characterInfo.CharacterSpellManager.equippedSpellNames.Length <= spellbookSlotIndex)
        {
            Debug.Log("Not enough equipped spells, casting cancelled.");
            return;
        }

        characterInfo.CharacterSpellManager.CastSpell(characterInfo.CharacterSpellManager.equippedSpellNames[spellbookSlotIndex]);
    }


    
    private void Update()
    {
        UpdateCooldown();
    }
    private void UpdateCooldown()
    {
        // Set up cooldowns if data is invalid
        if (spellCooldowns == null || spellCooldowns.Length < characterInfo.CharacterSpellManager.equippedSpellNames.Length)
        {
            spellCooldowns = new float[characterInfo.CharacterSpellManager.equippedSpellNames.Length];
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
            SetCooldownUI(i, spellCooldowns[i] / characterInfo.CharacterSpellManager.EquippedSpellData[i].SpellCooldown);
        }
    }
    private void SetCooldownUI(int index, float percentFilled)
    {
        // Gets the top bar GameObject
        GameObject bottomBar = spellDisplays[index].transform.GetChild(0).gameObject;
        GameObject topBar = bottomBar.transform.GetChild(0).gameObject;

        topBar.GetComponent<Image>().fillAmount = percentFilled;
    }
    private void UpdateSpellbookUI()
    {
        if (spellDisplays.Length < characterInfo.CharacterSpellManager.equippedSpellNames.Length)
        {
            Debug.LogWarning("Too many equipped spells! Final spells will not be rendered.");
        }

        // Loop through and update each sprite using the data from 
        for (var i = 0; i < spellDisplays.Length; i++)
        {
            if (characterInfo.CharacterSpellManager.equippedSpellNames.Length <= i)
            {
                //Debug.Log("No equipped spell in slot, skipping render");
                spellDisplays[i].SetActive(false);
                continue;
            }
            spellDisplays[i].GetComponent<SpriteRenderer>().enabled = true;
            spellDisplays[i].GetComponent<SpriteRenderer>().sprite = characterInfo.CharacterSpellManager.EquippedSpellData[i].Icon;
            //Debug.Log($"UI {i} updated");
        }

    }

}
