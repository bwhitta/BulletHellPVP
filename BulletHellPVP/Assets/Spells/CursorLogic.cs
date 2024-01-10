using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorLogic : NetworkBehaviour
{
    // Location
    private float location = 0;

    // Controls
    private InputAction cursorMovementInput, cursorAccelerationInput;

    // Networking
    private readonly NetworkVariable<float> serverSideLocation = new();
    private float previousServerSidePosition;
    private int ticksSincePositionUpdate;

    // References
    [SerializeField] private SpellManager spellManager; // Assigned in the inspector on the prefab, so it is instantiated with the reference.
    
    // Startup
    private void Start()
    {
        // Check to make sure reference is in place!
        if (spellManager == null) Debug.LogWarning("No reference given for Spell Manager on the SpellcastingObject prefab.");

        NetworkVariableListeners(); // Look out for when network variables change

        void NetworkVariableListeners()
        {
            if (!IsOwner)
            {
                serverSideLocation.OnValueChanged += ServerSideLocationUpdate;
            }
        }
        void ServerSideLocationUpdate(float prevLocation, float newLocation)
        {
            previousServerSidePosition = prevLocation;
            ticksSincePositionUpdate = 0;
        }
    }
    public void CharacterInfoSet()
    {
        // Set tag
        gameObject.tag = spellManager.characterInfo.CharacterAndSortingTag;

        //Get the InputActionMap
        InputActionMap controlsMap = ControlsManager.GetActionMap(spellManager.characterInfo.InputMapName);

        // Find input actions
        cursorMovementInput = controlsMap.FindAction(GameSettings.Used.CursorMovementInputName, true);
        cursorAccelerationInput = controlsMap.FindAction(GameSettings.Used.AccelerateCursorInputName, true);
        
        // Enable input
        cursorMovementInput.Enable();
        cursorAccelerationInput.Enable();
    }

    // Methods called each frame
    private void FixedUpdate()
    {
        float cursorMovement = cursorMovementInput.ReadValue<float>() * Time.fixedDeltaTime;
        cursorMovement *= GameSettings.Used.CursorMovementSpeed;
        if (cursorAccelerationInput.ReadValue<float>() >= 0.5f)
        {
            cursorMovement *= GameSettings.Used.CursorAcceleratedMovementMod;
        }

        if (IsServer)
        {
            ServerPositionTick();
        }
        
        MovementTick(cursorMovement);
    }
    private void MovementTick(float movement)
    {
        // Online movement tick
        if (MultiplayerManager.IsOnline)
        {
            if (IsOwner) OwnerMovementTick();
            else if (!IsServer) OpponentTick();
        }

        // Local movement tick
        if (MultiplayerManager.multiplayerType == MultiplayerManager.MultiplayerTypes.Local)
        {
            location += movement;
        }

        UpdateCursor();

        // Local Methods
        void OwnerMovementTick()
        {
            location += movement;
            if (!IsHost && IsClient)
            {
                MoveCursorServerRpc(movement, location);
            }
        }
        void OpponentTick()
        {
            ticksSincePositionUpdate++;
            float cappedTicks = Mathf.Min(ticksSincePositionUpdate, GameSettings.Used.NetworkDiscrepancyCheckFrequency);
            float interpolatePercent = cappedTicks / GameSettings.Used.NetworkDiscrepancyCheckFrequency;
            location = Calculations.RelativeTo(previousServerSidePosition, serverSideLocation.Value, interpolatePercent);
        }
    }
    private void ServerPositionTick()
    {
        ticksSincePositionUpdate++;
        if (ticksSincePositionUpdate >= GameSettings.Used.NetworkDiscrepancyCheckFrequency)
        {
            serverSideLocation.Value = location;

            ticksSincePositionUpdate = 0;
        }
    }
    
    /// <summary> Visually updates the cursor </summary>
    public void UpdateCursor()
    {
        float locationAroundSquare = Calculations.Modulo(location, GameSettings.Used.BattleSquareWidth * 4);

        int sideNumber = GetCurrentWall();
        
        float locationAroundSide = locationAroundSquare % GameSettings.Used.BattleSquareWidth;

        transform.rotation = Quaternion.Euler(0, 0, -90 * (sideNumber)); // Negative because it rotates counterclockwise

        Vector2[] corners = GetCurrentSquareCorners();

        int[,] modifierDirection = { { 1, 0 }, { 0, -1 }, { -1, 0 }, { 0, 1 } }; // Starts in top left, continues clockwise
        Vector2 positionModifier = new(locationAroundSide * modifierDirection[sideNumber, 0], locationAroundSide * modifierDirection[sideNumber, 1]);

        transform.position = corners[sideNumber] + positionModifier;
    }

    /// <summary> Gets the current corners of the square the CursorLogic this method is called for is attached to</summary>
    /// <returns> A list of the coordinates of the square. Starts in top left, continues clockwise </returns>
    public Vector2[] GetCurrentSquareCorners()
    {
        Vector2 pos = new()
        {
            x = spellManager.characterInfo.OpponentAreaCenterX,
            y = spellManager.characterInfo.OpponentAreaCenterY
        };
        return GetSquareCorners(GameSettings.Used.BattleSquareWidth, pos);
    }
    private Vector2[] GetSquareCorners(float sideLength, Vector2 centerPoint)
    {
        Vector2[] corners = new Vector2[4];

        int[,] cornerDirection = { { -1, 1 }, { 1, 1 }, { 1, -1 }, { -1, -1 } }; // Starts in top left, continues clockwise

        for (int i = 0; i < 4; i++)
        {
            corners[i].x = centerPoint.x + (sideLength * cornerDirection[i, 0] * 0.5f);
            corners[i].y = centerPoint.y + (sideLength * cornerDirection[i, 1] * 0.5f);
        }
        return corners;
    }
    public int GetCurrentWall()
    {
        float squareSide = GameSettings.Used.BattleSquareWidth;
        float locationAroundSquare = Calculations.Modulo(location, squareSide * 4f);

        return (int)Mathf.Floor(locationAroundSquare / squareSide);
    }

    // Server and Client Rpcs
    [ServerRpc]
    private void MoveCursorServerRpc(float input, float clientLocation)
    {
        location += input;
        float discrepancy = clientLocation - location;
        if (discrepancy >= GameSettings.Used.NetworkLocationDiscrepancyLimit)
        {
            Debug.LogWarning($"{name} has a discrepancy of {discrepancy}");
            FixDiscrepancyClientRpc(discrepancy);
        }
    }
    [ClientRpc]
    private void FixDiscrepancyClientRpc(float discrepancy)
    {
        Debug.LogWarning($"Client location wrong (discrepancy {discrepancy}).");
        if (IsOwner)
        {
            location -= discrepancy;
        }
    }
}
