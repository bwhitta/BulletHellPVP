using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SpellbookLogic : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] spellDisplays;
    [SerializeField] private GameObject bookNumberTextObject;
    private Text _bookNumberText;
    private Text BookNumberText
    {
        get
        {
            _bookNumberText = _bookNumberText != null ? _bookNumberText : bookNumberTextObject.GetComponent<Text>();
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
            castingAction.performed += context => CastingInputPerformed((byte)(castingAction.ReadValue<float>() - 1f));
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
        BookNumberText.text = characterInfo.CurrentBookIndex + 1.ToString();

        characterInfo.CreateBooks();

        // Loop through and update each sprite using the data from 
        for (int i = 0; i < spellDisplays.Length; i++)
        {
            if (characterInfo.CurrentBook == null)
            {
                spellDisplays[i].gameObject.SetActive(false);
                continue;
            }
            spellDisplays[i].enabled = true;
            spellDisplays[i].sprite = SpellDataFromSlot((byte)i).Icon;
        }
    }

    private SpellData SpellDataFromSlot(byte slotIndex)
    {
        byte setIndex = characterInfo.CurrentBook.SetIndexes[slotIndex];
        byte spellIndex = characterInfo.CurrentBook.SpellIndexes[slotIndex];

        SpellSetInfo setInfo = GameSettings.Used.SpellSets[setIndex];
        SpellData spell = setInfo.spellsInSet[spellIndex];
        return spell;
    }

    private void NextBookInputPerformed()
    {
        if (GameSettings.Used.CanLoopBooks)
        {
            characterInfo.CurrentBookIndex = (byte)((characterInfo.CurrentBookIndex + 1) % GameSettings.Used.TotalBooks);
            
        }
        else
        {
            characterInfo.CurrentBookIndex = (byte)Mathf.Min(characterInfo.CurrentBookIndex + 1, GameSettings.Used.TotalBooks);
        }

        UpdateSpellbookUI();
    }
    private void CastingInputPerformed(byte spellbookSlotIndex)
    {
        if (gameObject.activeSelf == false)
        {
            Debug.Log($"Casting cancelled, spellbook disabled.");
            return;
        }
        if (MultiplayerManager.IsOnline)
        {
            Debug.Log($"Casting imput performed. Server will now attempt spell");
            characterInfo.CharacterSpellManager.AttemptSpellServerRpc(spellbookSlotIndex);
        }
        else
        {
            characterInfo.CharacterSpellManager.AttemptSpell(spellbookSlotIndex);
        }
    }

    private void Update()
    {
        UpdateCooldown();
    }
    private void UpdateCooldown()
    {
        // Set up cooldowns if data is invalid
        if (spellCooldowns == null || spellCooldowns.Length != GameSettings.Used.TotalSpellSlots)
        {
            spellCooldowns = new float[GameSettings.Used.TotalSpellSlots];
        }

        // Loop through cooldowns and serverTick down by time.deltatime
        if (spellCooldowns == new float[GameSettings.Used.TotalSpellSlots])
        {
            Debug.Log("skipping cooldown, all zero");
            return;
        }
        for (int i = 0; i < spellCooldowns.Length; i++)
        {
            if (spellCooldowns[i] > 0)
            {
                spellCooldowns[i] -= Time.deltaTime;
                SetCooldownUI(i, spellCooldowns[i] / SpellDataFromSlot((byte)i).SpellCooldown);
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
        if(GameSettings.Used == null)
        {
            Debug.LogWarning("Used Game Settings is null. Did you forget a reference in the Character Info?");
        }
        for (int i = 0; i < GameSettings.Used.TotalSpellSlots; i++)
        {
            SetCooldownUI(i, percentFilled);
        }
    }
}
