using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorLogic : NetworkBehaviour
{
    // Fields
    [HideInInspector] public float location = 0;
    private InputAction cursorMovementInput, cursorAccelerationInput;
    private SpellManager spellManager;
    private bool hasCharacterInfo = false;

    public void CharacterInfoSet()
    {
        spellManager = GetComponent<SpellManager>();
        tag = spellManager.characterInfo.CharacterAndSortingTag;

        // Input stuff
        InputActionMap controlsMap = ControlsManager.GetActionMap(spellManager.characterInfo.InputMapName);
        cursorMovementInput = controlsMap.FindAction(GameSettings.Used.CursorMovementInputName, true);
        cursorAccelerationInput = controlsMap.FindAction(GameSettings.Used.AccelerateCursorInputName, true);
        cursorMovementInput.Enable();
        cursorAccelerationInput.Enable();

        hasCharacterInfo = true;
    }
    private void FixedUpdate()
    {
        if (!hasCharacterInfo) return;

        float cursorMovement = cursorMovementInput.ReadValue<float>() * Time.fixedDeltaTime;
        bool acceleratorPressed = cursorAccelerationInput.ReadValue<float>() >= 0.5f;
        cursorMovement *= GameSettings.Used.CursorMovementSpeed;

        if (IsServer)
        {
            LocationUpdateClientRpc(location);
        }
        
        MovementTick(cursorMovement, acceleratorPressed);
    }
    private void MovementTick(float input, bool acceleratorPressed)
    {
        // Online movement tick
        if (MultiplayerManager.IsOnline && IsOwner)
        {
            OwnerMovementTick();
        }

        // Local movement tick
        if (MultiplayerManager.IsOnline == false)
        {
            float velocity = input;
            if (acceleratorPressed)
            {
                velocity *= GameSettings.Used.CursorAcceleratedMovementMod;
            }

            location += velocity;
        }

        UpdateCursor();

        // Local Methods
        void OwnerMovementTick()
        {
            float velocity = input;
            if (acceleratorPressed)
            {
                velocity *= GameSettings.Used.CursorAcceleratedMovementMod;
            }

            location += velocity;
            
            if (!IsHost && IsClient)
            {
                MoveCursorServerRpc(input, acceleratorPressed, location);
            }
        }
    }
    public void UpdateCursor()
    {
        // Turn the side into a rotation
        int sideNumber = GetSideAtPosition(location);
        transform.SetPositionAndRotation(GetCursorTransform(location, spellManager.characterInfo.OpponentAreaCenter), Quaternion.Euler(0, 0, -90 * (sideNumber)));
    }
    public static Vector2 GetCursorTransform(float position, Vector2 opponentAreaCenter)
    {
        int sideNumber = GetSideAtPosition(position);
        if (sideNumber < 0 || sideNumber >= 4)
        {
            Debug.LogWarning($"side number of {sideNumber} does not make sense. deleteme");
        }

        float locationAroundSquare = Calculations.Modulo(position, GameSettings.Used.BattleSquareWidth * 4);
        float locationAroundSide = locationAroundSquare % GameSettings.Used.BattleSquareWidth;

        Vector2[] corners = GetSquareCorners(GameSettings.Used.BattleSquareWidth, opponentAreaCenter);

        // Probably can rewrite 
        int[,] modifierDirection = { { 1, 0 }, { 0, -1 }, { -1, 0 }, { 0, 1 } }; // Starts in top left, continues clockwise
        Vector2 positionModifier = new(locationAroundSide * modifierDirection[sideNumber, 0], locationAroundSide * modifierDirection[sideNumber, 1]);
        
        return corners[sideNumber] + positionModifier;

        static Vector2[] GetSquareCorners(float sideLength, Vector2 centerPoint)
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
    }
    public static int GetSideAtPosition(float cursorPosition)
    {
        // Makes it so that regardless of how many loops around the game area the cursor has done in either direction, it just tells distance clockwise from the top left corner.
        float distanceAlongSide = Calculations.Modulo(cursorPosition, GameSettings.Used.BattleSquareWidth * 4f);

        int sideNumber = (int)Mathf.Floor(distanceAlongSide / GameSettings.Used.BattleSquareWidth);
        
        // Floating points can create a bug when the value is a really small negative number (e.g. -1e^-8). If that happens, this will fix it.
        if (sideNumber >= 4) sideNumber = 0;

        // Returns how many sides worth of distance the cursor is from the top left corner, going clockwise.
        return sideNumber;
    }

    // Server and Client Rpcs
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
        float velocity = input;
        if (acceleratorPressed)
        {
            velocity *= GameSettings.Used.CursorAcceleratedMovementMod;
        }

        location += velocity;
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