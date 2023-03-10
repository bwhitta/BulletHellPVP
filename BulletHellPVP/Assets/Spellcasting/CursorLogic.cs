using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static ControlsManager;

public class CursorLogic : MonoBehaviour
{
    [SerializeField] private float acceleratedMovementMultiplier;
    [SerializeField][Range(-30f, 30f)] private float location;

    // Movement
    [SerializeField] private float movementMultiplier;
    [SerializeField] private Transform squareTransform;
    public float squareSide;

    // Controls
    [SerializeField] private string controllingPlayerMapName, cursorMovementInputName, cursorAccelerationInputName;
    private InputAction cursorMovementInput, cursorAccelerationInput;

    private void OnValidate()
    {
        squareSide = squareTransform.localScale.x;
        UpdateCursor();
    }

    private void Start()
    {
        squareSide = squareTransform.localScale.x;

        //Get the InputActionMap
        InputActionMap controllingPlayerMap = ControlsManager.GetActionMap(controllingPlayerMapName);

        // Set and enable 
        cursorMovementInput = controllingPlayerMap.FindAction(cursorMovementInputName, true);
        cursorMovementInput.Enable();

        cursorAccelerationInput = controllingPlayerMap.FindAction(cursorAccelerationInputName, true);
        cursorAccelerationInput.Enable();
    }
    private void Update()
    {
        float cursorMovement = cursorMovementInput.ReadValue<float>();
        cursorMovement = cursorMovement * Time.deltaTime * movementMultiplier;
        if (cursorAccelerationInput.ReadValue<float>() >= 0.5f)
        {
            cursorMovement *= acceleratedMovementMultiplier;
        }

        MoveCursor(cursorMovement);
    }

    private void MoveCursor(float movement)
    {
        location += movement;

        UpdateCursor();
        
    }

    private void UpdateCursor()
    {

        float locationAroundSquare = Modulo(location, squareSide * 4);

        int sideNumber = GetCurrentWall();
        float locationAroundSide = locationAroundSquare % squareSide;

        transform.rotation = Quaternion.Euler(0, 0, -90 * (sideNumber)); // Negative because it rotates counterclockwise

        Vector2[] corners = GetCurrentSquareCorners();

        int[,] modifierDirection = { { 1, 0 }, { 0, -1 }, { -1, 0 }, { 0, 1 } }; // Starts in top left, continues clockwise
        Vector2 positionModifier = new(locationAroundSide * modifierDirection[sideNumber, 0], locationAroundSide * modifierDirection[sideNumber, 1]); 

        transform.position = corners[sideNumber] + positionModifier;
    }

    private Vector2[] GetSquareCorners(float sideLength, float posX, float posY)
    {

        Vector2[] corners = new Vector2[4];
        
        int[,] cornerDirection = {{-1, 1},{1, 1},{1, -1},{-1, -1}}; // Starts in top left, continues clockwise

        for (var i = 0; i < 4; i++)
        {
            corners[i].x = posX + (sideLength * cornerDirection[i, 0] * 0.5f);
            corners[i].y = posY + (sideLength * cornerDirection[i, 1] * 0.5f);
        }
        return corners;
    }
    
    /// <summary> Gets the current corners of the CursorLogic's attatched square this method is called for </summary>
    /// <returns> A list of the coordinates of the square. Starts in top left, continues clockwise </returns>
    public Vector2[] GetCurrentSquareCorners()
    {
        if (!Mathf.Approximately(squareTransform.localScale.x, squareTransform.localScale.y))
        {
            Debug.LogError("Square transform discrepancy");
        }
        return GetSquareCorners(squareTransform.localScale.x, squareTransform.localPosition.x, squareTransform.localPosition.y);
    }

    private float Modulo(float numberToModify, float modifyingNumber)
    {
        return numberToModify - modifyingNumber * (Mathf.Floor(numberToModify / modifyingNumber));
    }

    public int GetCurrentWall()
    {
        float squareSide = squareTransform.localScale.x;
        float locationAroundSquare = Modulo(location, squareSide * 4);

        return (int)Mathf.Floor(locationAroundSquare / squareSide);
    }
}
