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
    private CharacterStatusEffects characterStatusEffects;

    private int ticksSinceDiscrepancyCheck;

    // Methods
    private void Start()
    {
        Debug.Log($"interpolation is currently removed! probably should re-add, delete me later.");
        // References
        characterAnimator = GetComponent<Animator>();
        characterManager = GetComponent<CharacterManager>();
        characterStatusEffects = GetComponent<CharacterStatusEffects>();

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
        Vector3 movement = GameSettings.Used.CharacterMovementSpeed * characterStatusEffects.MoveSpeedModifier * movementInput.normalized;
        
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

        // could make it so that this only checks every few fixedupdate frames, but I shouldn't need to
        Vector2 discrepancy = (Vector2)transform.position - clientPosition;
        if (discrepancy.magnitude >= GameSettings.Used.NetworkLocationDiscrepancyLimit)
        {
            //Debug.LogWarning($"{name}'s client has a discrepancy of {discrepancy} - server pos {(Vector2)transform.position}, client pos {clientPosition}");
            //FixDiscrepancyClientRpc(transform.position);
        }
    }
    [Rpc(SendTo.Owner)]
    private void FixDiscrepancyClientRpc(Vector2 position)
    {
        Debug.LogWarning($"Client location wrong, new position is {position}");
        transform.position = (Vector3)position;
    }
}