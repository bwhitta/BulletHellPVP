using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.UI;

public class SpellbookLogic : NetworkBehaviour
{
    // Fields
    [SerializeField] private Image[] spellDisplays;
    [SerializeField] private Text bookNumber;
    [SerializeField] private CharacterManager characterManager;

    [HideInInspector] public float[] spellCooldowns;
    
    private byte CurrentBookIndex;
    public Spellbook CurrentBook => characterManager.OwnerInfo.EquippedBooks[CurrentBookIndex];
    private int ticksSinceDiscrepancyCheck;
    private readonly NetworkVariable<byte> ServerBookIndex = new();

    // Methods
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

        // Set position
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.localPosition = characterManager.OwnerInfo.SpellbookPosition;

        EnableControls();
        SetupCooldownUi();
        UpdateUi();
        
        // Local Methods
        void BookIndexUpdated(byte oldValue, byte newValue)
        {
            Debug.Log($"Book index updated to {newValue}");
            CurrentBookIndex = newValue;

            UpdateUi();
        }
        void SetupCooldownUi()
        {
            if (GameSettings.Used == null)
            {
                Debug.LogError("Used Game Settings is null. Did you forget a reference in the Character Info?");
                return;
            }
            for (int i = 0; i < GameSettings.Used.SpellSlots; i++)
            {
                SetCooldownUI(i, 0);
            }
        }
        void EnableControls()
        {
            InputActionMap controlsMap;
            InputAction nextBookAction;

            controlsMap = ControlsManager.GetActionMap(characterManager.OwnerInfo.InputMapName);
            nextBookAction = controlsMap.FindAction(characterManager.OwnerInfo.NextBookActionName, true);

            // Enable controls
            nextBookAction.Enable();
            nextBookAction.performed += context => NextBookInputPerformed();
        }
    }
    private void FixedUpdate()
    {
        if (IsServer)
        {
            ServerDiscrepancyTick();
        }
        CooldownTick();

        void ServerDiscrepancyTick()
        {
            ticksSinceDiscrepancyCheck++;
            if (ticksSinceDiscrepancyCheck >= GameSettings.Used.NetworkDiscrepancyCheckFrequency)
            {
                ServerBookIndex.Value = CurrentBookIndex;

                ticksSinceDiscrepancyCheck = 0;
            }
        }
    }
    private void CooldownTick()
    {
        // Set up cooldowns if data is invalid
        if (spellCooldowns == null || spellCooldowns.Length != GameSettings.Used.SpellSlots)
        {
            spellCooldowns = new float[GameSettings.Used.SpellSlots];
        }
        
        // Reduce time on each cooldown
        for (int i = 0; i < spellCooldowns.Length; i++)
        {
            if (spellCooldowns[i] > 0)
            {
                spellCooldowns[i] -= Time.fixedDeltaTime;
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
    private void UpdateUi()
    {
        if (bookNumber == null) Debug.LogWarning($"forgot to set reference after restructuring everything!!! deleteme.");

        // Update text
        bookNumber.text = (CurrentBookIndex + 1).ToString();

        characterManager.OwnerInfo.EquippedBooks = Spellbook.CreateBooks(GameSettings.Used.SpellSlots);

        // Loop through and update each sprite using the data from 
        for (int i = 0; i < spellDisplays.Length; i++)
        {
            if (CurrentBook == null)
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
        byte setIndex = CurrentBook.SetIndexes[slotIndex];
        byte spellIndex = CurrentBook.SpellIndexes[slotIndex];

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
    private void NextBook()
    {
        if (GameSettings.Used.CanLoopBooks)
        {
            CurrentBookIndex = (byte)((CurrentBookIndex + 1) % GameSettings.Used.TotalBooks);
        }
        else
        {
            CurrentBookIndex = (byte)Mathf.Min(CurrentBookIndex + 1, GameSettings.Used.TotalBooks - 1);
        }
    }
    [ServerRpc]
    private void NextBookInputServerRpc()
    {
        Debug.Log($"serverrpc resolution");
        NextBook();
        UpdateUi();
    }
    
}
