using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.UI;

public class SpellbookLogic : NetworkBehaviour
{
    // Fields
    public static Spellbook[][] EquippedBooks;

    [SerializeField] private string nextBookActionName;
    [SerializeField] private Image[] spellDisplays;
    [SerializeField] private Text bookNumber;
    [SerializeField] private CharacterManager characterManager;

    [HideInInspector] public float[] spellCooldowns;

    private byte currentBookIndex;
    private readonly NetworkVariable<byte> ServerBookIndex = new();

    // Properties
    public Spellbook[] CharacterBooks => EquippedBooks[characterManager.CharacterIndex];
    public Spellbook CurrentBook => CharacterBooks[currentBookIndex];

    // Methods
    private void Start()
    {
        if (MultiplayerManager.IsOnline && !IsServer)
        {
            ServerBookIndex.OnValueChanged += ServerBookIndexUpdated;
        }
        
        // Starting position
        GetComponent<RectTransform>().localPosition = characterManager.OwnerInfo.SpellbookPosition;

        // Set up the equipped books if spell selection was skipped (e.g. if the game is run in unity starting in the battle scene)
        EquippedBooks[characterManager.CharacterIndex] ??= Spellbook.CreateBooks(GameSettings.Used.SpellSlots);
        RefreshBookUi();
        SetupCooldownUi();
        EnableControls();
        
        // Local Methods
        void ServerBookIndexUpdated(byte oldValue, byte newValue)
        {
            Debug.Log($"Book index updated to {newValue}");
            currentBookIndex = newValue;
        }
        void SetupCooldownUi()
        {
            spellCooldowns = new float[GameSettings.Used.SpellSlots];
            for (byte i = 0; i < GameSettings.Used.SpellSlots; i++)
            {
                DisplayCooldown(i, 0);
            }
        }
        void EnableControls()
        {
            InputActionMap controlsMap = ControlsManager.GetActionMap(characterManager.InputMapName);
            InputAction nextBookAction = controlsMap.FindAction(nextBookActionName, true);
            nextBookAction.Enable();
            nextBookAction.performed += context => NextBookInputPerformed();
        }
    }
    private void FixedUpdate()
    {
        CooldownTick();
    }
    private void CooldownTick()
    {
        for (byte i = 0; i < spellCooldowns.Length; i++)
        {
            if (spellCooldowns[i] > 0)
            {
                spellCooldowns[i] -= Mathf.Max(Time.fixedDeltaTime, 0);
                DisplayCooldown(i, spellCooldowns[i] / SpellDataInSlot(i).SpellCooldown);
            }
        }
    }
    private void DisplayCooldown(byte cooldownBarIndex, float percentFilled)
    {
        GameObject bottomBar = spellDisplays[cooldownBarIndex].transform.GetChild(0).gameObject;
        GameObject topBar = bottomBar.transform.GetChild(0).gameObject;
        topBar.GetComponent<Image>().fillAmount = percentFilled;
    }
    private void RefreshBookUi()
    {
        bookNumber.text = (currentBookIndex + 1).ToString();
        
        for (byte i = 0; i < spellDisplays.Length; i++)
        {
            spellDisplays[i].sprite = SpellDataInSlot(i).Icon;
        }
    }
    private SpellData SpellDataInSlot(byte slotIndex)
    {
        byte setIndex = CurrentBook.SetIndexes[slotIndex];
        byte spellIndex = CurrentBook.SpellIndexes[slotIndex];

        SpellSetInfo setInfo = GameSettings.Used.SpellSets[setIndex];
        return setInfo.spellsInSet[spellIndex];
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
    }
    private void NextBook()
    {
        if (GameSettings.Used.CanLoopBooks)
        {
            currentBookIndex = (byte)((currentBookIndex + 1) % GameSettings.Used.TotalBooks);
        }
        else
        {
            currentBookIndex = (byte)Mathf.Min(currentBookIndex + 1, GameSettings.Used.TotalBooks - 1);
        }
        RefreshBookUi();
    }

    [ServerRpc]
    private void NextBookInputServerRpc()
    {
        NextBook();
        RefreshBookUi();
    }
}
