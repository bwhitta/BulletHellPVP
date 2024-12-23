using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.UI;

public class SpellbookLogic : NetworkBehaviour
{
    // Fields
    [SerializeField] private SpriteRenderer[] spellDisplays;
    [SerializeField] private GameObject bookNumberTextObject;

    private int ticksSinceDiscrepancyCheck;
    private readonly NetworkVariable<byte> ServerBookIndex = new();
    public readonly NetworkVariable<byte> networkCharacterId = new();
    
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
    [HideInInspector] public CharacterInfo characterInfo;
    [HideInInspector] public float[] spellCooldowns;

    private void Start()
    {
        if (MultiplayerManager.IsOnline)
        {
            // Get the character info
            characterInfo = GameSettings.Used.Characters[networkCharacterId.Value];

            // If info is updated by the server, also update that client-side
            if (!IsServer)
            {
                networkCharacterId.OnValueChanged += CharacterIdUpdated;
                ServerBookIndex.OnValueChanged += BookIndexUpdated;
            }
        }

        SetupGameObject();
        EnableControls();
        SetupCooldownUi();
        UpdateUi();
        
        // Local Methods
        void CharacterIdUpdated(byte prev, byte changedTo)
        {
            Debug.Log($"ID CHANGED: CharacterId is {networkCharacterId.Value}", this);
            characterInfo = GameSettings.Used.Characters[changedTo];

            SetupGameObject();
            UpdateUi();
        }
        void BookIndexUpdated(byte oldValue, byte newValue)
        {
            Debug.Log($"Book index updated to {newValue}");
            characterInfo.CurrentBookIndex = newValue;

            UpdateUi();
        }
        void SetupGameObject()
        {
            // Set tag
            Debug.Log($"Characterinfo: {characterInfo} DELETEME");
            Debug.Log($"characterobject: {characterInfo.CharacterObject} DELETEME");
            Debug.Log($"tag: {characterInfo.CharacterObject.tag} DELETEME");
            tag = characterInfo.CharacterObject.tag;

            GameObject mainCanvas = GameObject.FindGameObjectWithTag(characterInfo.MainCanvasTag);
            transform.SetParent(mainCanvas.transform);

            // Set position
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.localPosition = characterInfo.SpellbookPosition;
            rectTransform.localScale = characterInfo.SpellbookScale;
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
                ServerBookIndex.Value = characterInfo.CurrentBookIndex;

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
        controlsMap ??= ControlsManager.GetActionMap(characterInfo.InputMapName);
        castingAction ??= controlsMap.FindAction(characterInfo.CastingActionName, true);
        nextBookAction ??= controlsMap.FindAction(characterInfo.NextBookActionName, true);

        // Enable controls
        castingAction.Enable();
        castingAction.performed += context => CastingInputPerformed((byte)(castingAction.ReadValue<float>() - 1f));
        nextBookAction.Enable();
        nextBookAction.performed += context => NextBookInputPerformed();
    }

    private void UpdateUi()
    {
        // Update text
        BookNumberText.text = (characterInfo.CurrentBookIndex + 1).ToString();

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
            characterInfo.CurrentBookIndex = (byte)((characterInfo.CurrentBookIndex + 1) % GameSettings.Used.TotalBooks);
        }
        else
        {
            characterInfo.CurrentBookIndex = (byte) Mathf.Min(characterInfo.CurrentBookIndex + 1, GameSettings.Used.TotalBooks-1);
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
            Debug.Log($"Casting input performed. Server will now attempt spell. characterInfo: {characterInfo}. CharacterSpellManager: {characterInfo.SpellManagerScript}.");

            // potentially first check max mana here, and then deduct mana from here for the client-side if the player isn't the host
            SpellData spellData = SpellManager.GetSpellData(characterInfo.CurrentBook, spellbookSlotIndex);
            bool canCastSpell = characterInfo.SpellManagerScript.CooldownAndManaAvailable(spellData, spellbookSlotIndex, true);

            if (canCastSpell)
            {
                characterInfo.SpellManagerScript.AttemptSpellServerRpc(spellbookSlotIndex);
            }
            else
            {
                Debug.Log("Skipped casting spell - there is not enough mana or the spell is on cooldown.");
            }
        }
        else
        {
            characterInfo.SpellManagerScript.AttemptSpell(spellbookSlotIndex);
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
