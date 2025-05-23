using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/InputAction Names")]
public class InputActionNames : ScriptableObject
{
    public string[] InputMapNames;

    [Header("Character Movement")]
    public string Movement;

    [Header("Spellcasting")]
    public string CastingAction;

    [Header("Cursor")]
    public string CursorMovement;
    public string AccelerateCursor;
}
