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
    private void Start()
    {
        Debug.Log($"interpolation is currently removed! probably should re-add, delete me later.");
        // References
        characterAnimator = GetComponent<Animator>();
        characterManager = GetComponent<CharacterManager>();

        // Enable movement
        InputActionMap controlsMap = ControlsManager.GetActionMap(characterManager.InputMapName);
        movementAction = controlsMap.FindAction(GameSettings.InputNames.Movement, true);
        movementAction.Enable();

        // Starting position
        transform.position = GameSettings.Used.CharacterStartPositions[characterManager.CharacterIndex];
    }
    private void FixedUpdate()
    {
        MovementTick();
    }
    private void MovementTick()
    {
        Vector2 movementInput = movementAction.ReadValue<Vector2>();

        if (!MultiplayerManager.IsOnline)
        {
            MoveCharacter(movementInput);
        }
        else if (IsOwner)
        {
            MoveCharacter(movementInput);
            if (IsServer)
            {
                LocationUpdateClientRpc(transform.position);
            }
            else
            {
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

    // Networking
    [Rpc(SendTo.NotServer)]
    private void LocationUpdateClientRpc(Vector2 pos)
    {
        transform.position = pos;
    }
    [Rpc(SendTo.Server)]
    private void MoveCharacterServerRpc(Vector2 inputVector, Vector2 clientPosition)
    {
        MoveCharacter(inputVector);

        Vector2 discrepancy = clientPosition - (Vector2)transform.position;
        if (discrepancy.magnitude >= GameSettings.Used.NetworkLocationDiscrepancyLimit)
        {
            Debug.LogWarning($"{name} has a discrepancy of {discrepancy}");
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