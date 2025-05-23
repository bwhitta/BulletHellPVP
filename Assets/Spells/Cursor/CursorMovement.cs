using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorMovement : NetworkBehaviour
{
    // Fields
    [SerializeField] private CharacterManager characterManager;

    [HideInInspector] public float location = 0;
    private InputAction cursorMovementInput;
    private InputAction cursorAccelerationInput;

    // Methods
    private void Start()
    {
        InputActionMap controlsMap = ControlsManager.GetActionMap(characterManager.InputMapName);
        
        cursorMovementInput = controlsMap.FindAction(GameSettings.InputNames.CursorMovement, true);
        cursorAccelerationInput = controlsMap.FindAction(GameSettings.InputNames.AccelerateCursor, true);
        cursorMovementInput.Enable();
        cursorAccelerationInput.Enable();
    }
    private void FixedUpdate()
    {
        float movementInput = cursorMovementInput.ReadValue<float>() * Time.fixedDeltaTime;
        bool acceleratorInput = cursorAccelerationInput.ReadValue<float>() >= 0.5f;

        if (IsServer)
        {
            LocationUpdateClientRpc(location);
        }
        
        MovementTick(movementInput, acceleratorInput);
    }
    private void MovementTick(float input, bool acceleratorPressed)
    {
        if ((MultiplayerManager.IsOnline && IsOwner))
        {
            MoveCursor(input, acceleratorPressed);

            if (!IsHost && IsClient)
            {
                MoveCursorServerRpc(input, acceleratorPressed, location);
            }
        }
        else if (!MultiplayerManager.IsOnline)
        {
            MoveCursor(input, acceleratorPressed);
        }

        // Calculate and set the position and rotation based on location
        Vector2 position = CalculateCursorPosition(location, GameSettings.Used.BattleAreaCenters[characterManager.CharacterIndex]);
        Quaternion angle = Quaternion.Euler(0, 0, -90 * Calculations.SquareSideAtPosition(GameSettings.Used.BattleSquareWidth, location));
        transform.SetPositionAndRotation(position, angle);
    }
    private void MoveCursor(float input, bool acceleratorPressed)
    {
        float velocity = input * GameSettings.Used.CursorMovementSpeed;
        if (acceleratorPressed)
        {
            velocity *= GameSettings.Used.CursorAcceleratedMovementMod;
        }

        location += velocity;
    }

    public static Vector2 CalculateCursorPosition(float location, Vector2 opponentAreaCenter)
    {
        int sideNumber = Calculations.SquareSideAtPosition(GameSettings.Used.BattleSquareWidth, location);
        if (sideNumber < 0 || sideNumber >= 4)
        {
            Debug.LogWarning($"side number of {sideNumber} does not make sense. deleteme");
        }
        
        float locationAlongEdges = Calculations.Modulo(location, GameSettings.Used.BattleSquareWidth * 4);
        float locationAlongSide = locationAlongEdges % GameSettings.Used.BattleSquareWidth;

        // Starts in top left, continues clockwise
        Vector2[] corners = Calculations.GetSquareCorners(GameSettings.Used.BattleSquareWidth, opponentAreaCenter);
        Vector2[] cornerOffsetDirections = { new(1, 0), new(0, -1), new(-1, 0), new(0, 1) };
        Vector2 offsetFromCorner = locationAlongSide * cornerOffsetDirections[sideNumber];

        return corners[sideNumber] + offsetFromCorner;
    }

    [ClientRpc]
    private void LocationUpdateClientRpc(float locationAroundSquare)
    {
        // Host doesn't need to tell itself what its own value is, and if you are the owner then discrepancy checks will tell you where you should be.
        if (IsHost || IsOwner) return;
        location = locationAroundSquare;
    }
    [ServerRpc]
    private void MoveCursorServerRpc(float input, bool acceleratorPressed, float clientLocation)
    {
        // I might want to add something where it checks to make sure multiple cursor inputs can't be sent in a single frame
        MoveCursor(Mathf.Clamp(input, -1, 1), acceleratorPressed);
        
        float discrepancy = location - clientLocation;
        if (Mathf.Abs(discrepancy) >= GameSettings.Used.NetworkLocationDiscrepancyLimit)
        {
            Debug.LogWarning($"{name} has a discrepancy of {discrepancy}. Client's location: {clientLocation}, server estimate of location: {location}");
            FixDiscrepancyClientRpc(discrepancy);
        }
    }
    [ClientRpc]
    private void FixDiscrepancyClientRpc(float discrepancy)
    {
        Debug.LogWarning($"Location of this client is wrong (discrepancy {discrepancy}).");
        if (IsOwner)
        {
            location -= discrepancy;
        }
    }
}