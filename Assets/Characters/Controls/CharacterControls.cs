using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterControls : NetworkBehaviour
{
    // Fields
    [SerializeField] private string animatorTreeParameterX;
    [SerializeField] private string animatorTreeParameterY;
    [SerializeField] private string movementActionName;
    [SerializeField] private Vector2[] characterStartPositions; // RENAME TO CHARACTERSTARTPOSITIONS

    [HideInInspector] public InputAction movementAction;
    
    private Animator characterAnimator;
    private CharacterManager characterManager;
    
    public class TempMovementMod
    {
        public Vector2 tempPush = Vector2.zero;
        public float tempMovementMod = 1f;
        public bool removeEffect = false;
        // public ushort lifetime; Not using for now, should instead change removeEffect from the SpellModuleBehavior
    }
    public List<TempMovementMod> tempMovementMods = new();

    // Methods
    private void Awake()
    {
        characterAnimator = GetComponent<Animator>();
        characterManager = GetComponent<CharacterManager>();
    }
    private void Start()
    {
        Debug.Log($"interpolation is currently removed! probably should re-add, delete me later.");
        
        // Enable movement
        InputActionMap controlsMap = ControlsManager.GetActionMap(characterManager.InputMapName);
        movementAction = controlsMap.FindAction(movementActionName, true);
        movementAction.Enable();

        // Starting position
        transform.position = characterStartPositions[characterManager.CharacterIndex];
        if (IsServer) LocationUpdateClientRpc(transform.position);
    }
    private void FixedUpdate()
    {
        MovementTick();
        if (IsServer && IsOwner)
        {
            LocationUpdateClientRpc(transform.position);
        }
    }
    private void MovementTick()
    {
        Vector2 movementInput = movementAction.ReadValue<Vector2>();

        if (MultiplayerManager.IsOnline == false)
        {
            MoveCharacter(movementInput);
        }
        else if (IsOwner)
        {
            OwnerMovementTick();
        }

        // Counts down the remaining time on temporary movement effects
        TempMovementTick();

        // Local Methods
        void OwnerMovementTick()
        {
            if (IsHost)
            {
                MoveCharacter(movementInput);
            }
            else if (IsClient)
            {
                MoveCharacter(movementInput);
                MoveCharacterServerRpc(movementInput, transform.position);
            }
        }
        void TempMovementTick()
        {
            foreach (TempMovementMod tempMod in tempMovementMods)
            {
                if (tempMod.removeEffect)
                {
                    Debug.Log($"this seems to be set up in the dumbest possible way, why does the tempMod not just have a built in timer?? delete me later.");
                    tempMovementMods.Remove(tempMod);
                }
            }
        }
    }
    private void MoveCharacter(Vector2 movementInput)
    {
        Vector3 movement = GameSettings.Used.CharacterMovementSpeed * CalculateTempMovementMod() * movementInput.normalized;

        transform.position += (movement + (Vector3)CalculateTempPush()) * Time.fixedDeltaTime;

        characterAnimator.SetFloat(animatorTreeParameterX, movementInput.x);
        characterAnimator.SetFloat(animatorTreeParameterY, movementInput.y);

        // Local Methods
        float CalculateTempMovementMod()
        {
            float movementMod = 1f;
            foreach (TempMovementMod tempMod in tempMovementMods)
            {
                movementMod *= tempMod.tempMovementMod;
            }
            return movementMod;
        }
        Vector2 CalculateTempPush()
        {
            Vector2 movementMod = Vector2.zero;
            foreach (TempMovementMod tempMod in tempMovementMods)
            {
                movementMod += tempMod.tempPush;
            }
            return movementMod;
        }
    }

    // Rpcs
    [ClientRpc]
    private void LocationUpdateClientRpc(Vector2 pos)
    {
        if (IsHost) return;
        transform.position = pos;
    }
    [ServerRpc]
    private void MoveCharacterServerRpc(Vector2 inputVector, Vector2 clientPosition)
    {
        MoveCharacter(inputVector);

        Vector2 discrepancy = clientPosition - (Vector2)transform.position;
        if (discrepancy.magnitude >= GameSettings.Used.NetworkLocationDiscrepancyLimit)
        {
            Debug.Log($"{name} has a discrepancy of {discrepancy}");
            FixDiscrepancyClientRpc(discrepancy);
        }
    }
    [ClientRpc]
    private void FixDiscrepancyClientRpc(Vector2 discrepancy)
    {
        Debug.Log($"Client location wrong (discrepancy {discrepancy}).");
        if (IsOwner)
        {
            transform.position -= (Vector3)discrepancy;
        }
    }
}