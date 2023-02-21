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
    [SerializeField][Range(0f, 30f)] private float location;
    [SerializeField] private Transform squareTransform;

    private void Start()
    {
        controls = GameControlsMaster.GameControls;
    }
    private void Update()
    {
        float cursorMovement = controls.Player.CursorMovement.ReadValue<float>();
        if (controls.Player.AccelerateCursorMovement.ReadValue<bool>())
        {
            cursorMovement = cursorMovement * Time.deltaTime * movementMultiplier * acceleratedMovementMultiplier;
        }
        else
        {
            cursorMovement = cursorMovement * Time.deltaTime * movementMultiplier;
        }
        MoveCursor(cursorMovement);
    }

    private void MoveCursor(float movement)
    {
        float squareSide = squareTransform.localScale.x;

        location = (location + movement) % (squareSide * 4);

        UpdateCursor();
        
    }

    private void UpdateCursor()
    {
        float squareSide = squareTransform.localScale.x;
        float posX = squareTransform.localPosition.x;
        float posY = squareTransform.localPosition.y;

        if (Mathf.Approximately(squareSide, squareTransform.localScale.y))
        {
            Debug.LogError("Square transform discrepancy");
        }

        float positionAlongSide = location % squareSide;
        int sideNumber = (int) (Mathf.Floor(location / squareSide));


        this.transform.rotation = Quaternion.Euler(0, 90 * (sideNumber-1), 0);
        
    }

    private Vector2[] GetSquareCorners(float sideLength, float posX, float posY)
    {
        Vector2[] corners = new Vector2[4];
        
        int[,] cornerDirection = {{-1, 1},{1, 1},{1, -1},{-1, -1}}; // Starts in top left, continues clockwise
        
        for (var i = 0; i < 4; i++)
        {
            corners[i].x = posX + sideLength * cornerDirection[i, 1] * 0.5f;
            corners[i].y = posY + sideLength * cornerDirection[i, 2] * 0.5f;
        }
        return corners;
    }
}
