using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SpellbookLogic : MonoBehaviour
{
    [SerializeField] private GameObject[] spellDisplays;
    [SerializeField] private GameObject bookNumberTextObject;
    private Text _bookNumberText;
    private Text BookNumberText
    {
        get
        {
            _bookNumberText ??= bookNumberTextObject.GetComponent<Text>();
            return _bookNumberText;
        }
    }
    [SerializeField] private CharacterInfo characterInfo;
    [HideInInspector] public float[] spellCooldowns;

    private void Awake()
    {
        gameObject.tag = characterInfo.CharacterAndSortingTag;
    }
    private void Start()
    {
        // Turn off spellbook if character doesn't exist
        if (characterInfo.CharacterObject == null)
        {
            SpellbookToggle(false);
        }

        AllCooldownUIs();
    }
    public void SpellbookToggle(bool enable)
    {
        gameObject.SetActive(enable);
        EnableSpellControls(enable);
        EnableBookControl(enable);
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
    private InputAction nextBookAction;
    private void EnableSpellControls(bool enable)
    {
        if (enable)
        {
            FindControls();
            // Enable controls
            castingAction.Enable();
            Debug.Log($"Casting Action: {castingAction}");
            castingAction.performed += context => CastingInputPerformed((int)castingAction.ReadValue<float>() - 1);
        }
        else
        {
            FindControls();
            // Disable controls
            castingAction.Disable();
        }

        // Local Methods
        void FindControls()
        {
            controlsMap ??= ControlsManager.GetActionMap(characterInfo.InputMapName);
            castingAction ??= controlsMap.FindAction(characterInfo.CastingActionName, true);
        }
    }
    private void EnableBookControl(bool enable)
    {
        if (enable)
        {
            FindControls();
            // Enable controls
            nextBookAction.Enable();
            nextBookAction.performed += context => NextBookInputPerformed();
        }
        else
        {
            FindControls();
            // Disable controls
            nextBookAction.Disable();
        }

        // Local Methods
        void FindControls()
        {
            controlsMap ??= ControlsManager.GetActionMap(characterInfo.InputMapName);
            nextBookAction ??= controlsMap.FindAction(characterInfo.NextBookActionName, true);
        }
    }
    private void UpdateSpellbookUI()
    {
        // Update text
        BookNumberText.text = characterInfo.CurrentBook + 1.ToString();

        if (characterInfo.EquippedSpellBooks == null)
        {
            characterInfo.CreateBooks();
        }
        // Loop through and update each sprite using the data from 
        for (var i = 0; i < spellDisplays.Length; i++)
        {
            if (characterInfo.EquippedSpellBooks[characterInfo.CurrentBook].Length <= i || characterInfo.EquippedSpellBooks[characterInfo.CurrentBook][i] == null)
            {
                spellDisplays[i].SetActive(false);
                continue;
            }
            SpriteRenderer spriteRenderer = spellDisplays[i].GetComponent<SpriteRenderer>();
            spriteRenderer.enabled = true;
            spellDisplays[i].GetComponent<SpriteRenderer>().sprite = characterInfo.EquippedSpellBooks[characterInfo.CurrentBook][i].Icon;
        }
    }

    private void NextBookInputPerformed()
    {
        if (characterInfo.UsedGameSettings.CanLoopBooks)
        {
            characterInfo.CurrentBook = Mathf.Min((characterInfo.CurrentBook + 1), characterInfo.EquippedSpellBooks.Length);
        }
        else
        {
            characterInfo.CurrentBook = (characterInfo.CurrentBook + 1) % characterInfo.EquippedSpellBooks.Length;
        }
        
        UpdateSpellbookUI();
    }
    private void CastingInputPerformed(int spellbookSlotIndex)
    {
        Debug.Log("Casting input performed");
        if (gameObject.activeSelf == false)
        {
            Debug.Log($"Casting cancelled, spellbook disabled.");
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
        if (spellCooldowns == null || spellCooldowns.Length != characterInfo.UsedGameSettings.TotalSpellSlots)
        {
            spellCooldowns = new float[characterInfo.UsedGameSettings.TotalSpellSlots];
        }

        // Loop through cooldowns and tick down by time.deltatime
        if (spellCooldowns == new float[characterInfo.UsedGameSettings.TotalSpellSlots])
        {
            Debug.Log("skipping cooldown, all zero");
            return;
        }
        for (int i = 0; i < spellCooldowns.Length; i++)
        {
            if (spellCooldowns[i] > 0)
            {
                spellCooldowns[i] -= Time.deltaTime;
                SetCooldownUI(i, spellCooldowns[i] / characterInfo.EquippedSpellBooks[characterInfo.CurrentBook][i].SpellCooldown);
            }
            if (spellCooldowns[i] < 0)
            {
                spellCooldowns[i] = 0;
            }
        }
    }
    private void SetCooldownUI(int index, float percentFilled)
    {
        // Gets the top bar GameObject
        GameObject bottomBar = spellDisplays[index].transform.GetChild(0).gameObject;
        GameObject topBar = bottomBar.transform.GetChild(0).gameObject;

        topBar.GetComponent<Image>().fillAmount = percentFilled;
    }
    private void AllCooldownUIs(float percentFilled = 0)
    {
        if(characterInfo.UsedGameSettings == null)
        {
            Debug.LogWarning("Used Game Settings is null. Did you forget a reference in the Character Info?");
        }
        for (int i = 0; i < characterInfo.UsedGameSettings.TotalSpellSlots; i++)
        {
            SetCooldownUI(i, percentFilled);
        }
    }
}
