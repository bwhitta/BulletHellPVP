using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.UI;

public class SpellbookLogic : NetworkBehaviour
{
    // Fields
    [SerializeField] private Image[] spellDisplays;
    [SerializeField] private GameObject bookNumberTextObject;
    [SerializeField] private CharacterManager characterManager;

    private int ticksSinceDiscrepancyCheck;
    private readonly NetworkVariable<byte> ServerBookIndex = new();
    
    // Properties
    private Text _bookNumberText;
    private Text BookNumberText
    {
        get
        {
            _bookNumberText = _bookNumberText != null ? _bookNumberText : bookNumberTextObject.GetComponent<Text>();
            return _bookNumberText;
        }
    }

    [HideInInspector] public float[] spellCooldowns;
    [SerializeField] private SpellManager spellManager;

    private void Start()
    {
        Debug.Log($"Note to self: tracking the current book and stuff should probably be done outside of CharacterInfo, and then this probably doesn't need to access characterManager nearly as much. " +
            $"Also, this script (SpellbookLogic) probably can be split into several smaller parts. Delete me later. ");
        if (MultiplayerManager.IsOnline)
        {
            // If info is updated by the server, also update that client-side
            if (!IsServer)
            {
                ServerBookIndex.OnValueChanged += BookIndexUpdated;
            }
        }

        SetupGameObject();
        EnableControls();
        SetupCooldownUi();
        UpdateUi();
        
        // Local Methods
        void BookIndexUpdated(byte oldValue, byte newValue)
        {
            Debug.Log($"Book index updated to {newValue}");
            characterManager.OwnedCharacterInfo.CurrentBookIndex = newValue;

            UpdateUi();
        }
        void SetupGameObject()
        {
            // Set position
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.localPosition = characterManager.OwnedCharacterInfo.SpellbookPosition;
        }
        void SetupCooldownUi()
        {
            if (GameSettings.Used == null)
            {
                Debug.LogError("Used Game Settings is null. Did you forget a reference in the Character Info?");
                return;
            }
            for (int i = 0; i < GameSettings.Used.TotalSpellSlots; i++)
            {
                SetCooldownUI(i, 0);
            }
        }
    }
    private void FixedUpdate()
    {
        if (IsServer)
        {
            ServerDiscrepancyTick();
        }

        void ServerDiscrepancyTick()
        {
            ticksSinceDiscrepancyCheck++;
            if (ticksSinceDiscrepancyCheck >= GameSettings.Used.NetworkDiscrepancyCheckFrequency)
            {
                ServerBookIndex.Value = characterManager.OwnedCharacterInfo.CurrentBookIndex;

                ticksSinceDiscrepancyCheck = 0;
            }
        }
    }

    // Enabling spell controls
    private InputActionMap controlsMap;
    private InputAction castingAction;
    private InputAction nextBookAction;
    private void EnableControls()
    {
        // Find the controls
        controlsMap ??= ControlsManager.GetActionMap(characterManager.OwnedCharacterInfo.InputMapName);
        castingAction ??= controlsMap.FindAction(characterManager.OwnedCharacterInfo.CastingActionName, true);
        nextBookAction ??= controlsMap.FindAction(characterManager.OwnedCharacterInfo.NextBookActionName, true);

        // Enable controls
        castingAction.Enable();
        castingAction.performed += context => CastingInputPerformed((byte)(castingAction.ReadValue<float>() - 1f));
        nextBookAction.Enable();
        nextBookAction.performed += context => NextBookInputPerformed();
    }

    private void UpdateUi()
    {
        // Update text
        BookNumberText.text = (characterManager.OwnedCharacterInfo.CurrentBookIndex + 1).ToString();

        characterManager.OwnedCharacterInfo.CreateBooks();

        // Loop through and update each sprite using the data from 
        for (int i = 0; i < spellDisplays.Length; i++)
        {
            if (characterManager.OwnedCharacterInfo.CurrentBook == null)
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
        byte setIndex = characterManager.OwnedCharacterInfo.CurrentBook.SetIndexes[slotIndex];
        byte spellIndex = characterManager.OwnedCharacterInfo.CurrentBook.SpellIndexes[slotIndex];

        SpellSetInfo setInfo = GameSettings.Used.SpellSets[setIndex];
        SpellData spell = setInfo.spellsInSet[spellIndex];
        return spell;
    }

    private void NextBookInputPerformed()
    {
        if (!MultiplayerManager.IsOnline || IsServer)
        {
            NextBook();
        }
        else
        {
            NextBookInputServerRpc();
        }
        UpdateUi();
    }

    [ServerRpc]
    private void NextBookInputServerRpc()
    {
        Debug.Log($"serverrpc resolution");
        NextBook();
        UpdateUi();
    }
    
    private void NextBook()
    {
        if (GameSettings.Used.CanLoopBooks)
        {
            characterManager.OwnedCharacterInfo.CurrentBookIndex = (byte)((characterManager.OwnedCharacterInfo.CurrentBookIndex + 1) % GameSettings.Used.TotalBooks);
        }
        else
        {
            characterManager.OwnedCharacterInfo.CurrentBookIndex = (byte) Mathf.Min(characterManager.OwnedCharacterInfo.CurrentBookIndex + 1, GameSettings.Used.TotalBooks-1);
        }
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
            // potentially first check max mana here, and then deduct mana from here for the client-side if the player isn't the host
            SpellData spellData = SpellManager.GetSpellData(characterManager.OwnedCharacterInfo.CurrentBook, spellbookSlotIndex);
            bool canCastSpell = spellManager.CooldownAndManaAvailable(spellData, spellbookSlotIndex, true);

            if (canCastSpell)
            {
                spellManager.AttemptSpellServerRpc(spellbookSlotIndex);
            }
            else
            {
                Debug.Log("Skipped casting spell - there is not enough mana or the spell is on cooldown.");
            }
        }
        else
        {
            spellManager.AttemptSpell(spellbookSlotIndex);
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
}
