using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterControls : NetworkBehaviour
{
    // Fields
    [SerializeField] private string animatorTreeParameterX;
    [SerializeField] private string animatorTreeParameterY;

    [HideInInspector] public InputAction movementAction;
    
    private Animator characterAnimator;
    private CharacterManager characterManager;

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
        movementAction = controlsMap.FindAction(GameSettings.InputNames.Movement, true);
        movementAction.Enable();

        // Starting position
        transform.position = GameSettings.Used.CharacterStartPositions[characterManager.CharacterIndex];
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
    }
    private void MoveCharacter(Vector2 movementInput)
    {
        Vector3 movement = GameSettings.Used.CharacterMovementSpeed * movementInput.normalized;

        transform.position += movement * Time.fixedDeltaTime;

        characterAnimator.SetFloat(animatorTreeParameterX, movementInput.x);
        characterAnimator.SetFloat(animatorTreeParameterY, movementInput.y);

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