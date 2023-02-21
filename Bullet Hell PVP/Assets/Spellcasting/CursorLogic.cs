using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static ControlsManager;

public class CursorLogic : MonoBehaviour
{
    [SerializeField] private float movementMultiplier;
    [SerializeField] private float acceleratedMovementMultiplier;
    GameControls controls;
    [SerializeField][Range(-30f, 30f)] private float location;
    [SerializeField] private Transform squareTransform;

    [SerializeField] private InputAction cursorMovement, cursorAccelerate;

    private void OnValidate()
    {
        UpdateCursor();
    }

    private void Start()
    {
        controls = GameControlsMaster.GameControls;

        cursorMovement = controls.Player.CursorMovement;
        cursorMovement.Enable();

        cursorAccelerate = controls.Player.AccelerateCursorMovement;
        cursorAccelerate.Enable();
    }
    private void Update()
    {
        float cursorMovement = controls.Player.CursorMovement.ReadValue<float>();
        cursorMovement = cursorMovement * Time.deltaTime * movementMultiplier;
        if (cursorAccelerate.ReadValue<float>() >= 0.5f)
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
        float squareSide = squareTransform.localScale.x;
        float squarePosX = squareTransform.localPosition.x;
        float squarePosY = squareTransform.localPosition.y;
        if (!Mathf.Approximately(squareSide, squareTransform.localScale.y))
        {
            Debug.LogError("Square transform discrepancy");
        }

        float locationAroundSquare = Modulo(location, squareSide * 4);
        // location % (squareSide * 4);

        int sideNumber = (int) Mathf.Floor(locationAroundSquare/squareSide);
        float locationAroundSide = locationAroundSquare % squareSide;

        transform.rotation = Quaternion.Euler(0, 0, -90 * (sideNumber)); // Negative because it rotates counterclockwise

        Vector2[] corners = GetSquareCorners(squareSide, squarePosX, squarePosY);

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

    private float Modulo(float numberToModify, float modifyingNumber)
    {
        return numberToModify - modifyingNumber * (Mathf.Floor(numberToModify / modifyingNumber));
    }
}
