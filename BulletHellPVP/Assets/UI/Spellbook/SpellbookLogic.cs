using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SpellbookLogic : MonoBehaviour
{
    [SerializeField] private GameObject spellManagerObject;
    [SerializeField] private GameObject[] spellDisplays;
    [SerializeField] private PlayerInfo playerInfo;
    [HideInInspector] public float[] spellCooldowns;

    private void Start()
    {
        if (playerInfo.PlayerObject == null)
        {
            SpellbookToggle(false);
        }
        else
        {
            SpellbookToggle(true);
        }
    }

    public void SpellbookToggle(bool enabled)
    {
        gameObject.SetActive(enabled);
        EnableSpellControls();
        if (enabled)
        {
            UpdateSpellbookUI();
        }
        else
        {
            playerInfo.SpellbookLogicScript = this;
        }
    }

    private void UpdateSpellbookUI()
    {
        if (spellDisplays.Length < playerInfo.CharacterSpellManager.equippedSpellNames.Length)
        {
            Debug.LogWarning("Too many equipped spells! Final spells will not be rendered.");
        }

        // Loop through and update each sprite using the data from 
        for (var i = 0; i < spellDisplays.Length; i++)
        {
            if (playerInfo.CharacterSpellManager.equippedSpellNames.Length <= i) {
                //Debug.Log("No equipped spell in slot, skipping render");
                spellDisplays[i].SetActive(false);
                continue;
            }
            spellDisplays[i].GetComponent<SpriteRenderer>().enabled = true;
            spellDisplays[i].GetComponent<SpriteRenderer>().sprite = playerInfo.CharacterSpellManager.EquippedSpellData[i].SpellbookSprite;
            //Debug.Log($"UI {i} updated");
        }
        
    }

    private void Update()
    {
        UpdateCooldown();
    }
    private void UpdateCooldown()
    {
        // Set up cooldowns if data is invalid
        if (spellCooldowns == null || spellCooldowns.Length < playerInfo.CharacterSpellManager.equippedSpellNames.Length)
        {
            spellCooldowns = new float[playerInfo.CharacterSpellManager.equippedSpellNames.Length];
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
            UpdateCooldownUI(i, spellCooldowns[i] / playerInfo.CharacterSpellManager.EquippedSpellData[i].SpellCooldown);
        }
    }
    private void UpdateCooldownUI(int index, float percentFilled)
    {
        // Gets the top bar GameObject
        GameObject bottomBar = spellDisplays[index].transform.GetChild(0).gameObject;
        GameObject topBar = bottomBar.transform.GetChild(0).gameObject;

        topBar.GetComponent<Image>().fillAmount = percentFilled;
    }

    public void EnableSpellControls ()
    {
        InputActionMap controllingPlayerMap = ControlsManager.GetActionMap(playerInfo.InputMapName);
        InputAction castingAction = controllingPlayerMap.FindAction(playerInfo.SpellbookSelectionActionName, true);
            
        castingAction.Enable();
        castingAction.performed += context => CastingInputPerformed((int)castingAction.ReadValue<float>() - 1);
    }

    private void CastingInputPerformed(int spellbookSlotIndex)
    {
        if (gameObject.activeSelf == false)
        {
            Debug.Log($"Casting cancelled, spellbook disabled.");
            return;
        }
        if (playerInfo.CharacterSpellManager.equippedSpellNames.Length <= spellbookSlotIndex)
        {
            Debug.LogWarning("Error - not enough equipped spells.");
            return;
        }

        playerInfo.CharacterSpellManager.CastSpell(playerInfo.CharacterSpellManager.equippedSpellNames[spellbookSlotIndex]);
    }
}
