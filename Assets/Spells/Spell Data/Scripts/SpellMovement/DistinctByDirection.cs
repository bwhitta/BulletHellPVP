using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Spell Movement/Distinct Move By Direction")]
public class DistinctByDirection : SpellMovement
{
    // could add a line of code (like a modulo) to make it loop through these offsets if 
    [SerializeField] private float[] distinctRotationOffsets;

    public override Vector2 Move(float angle, byte moduleObjectIndex)
    {
        int loopedIndex = moduleObjectIndex % distinctRotationOffsets.Length;

        float offsetAngle = angle + RotationOffset + distinctRotationOffsets[loopedIndex];
        
        Vector2 moveDirection = Quaternion.Euler(0, 0, offsetAngle) * Vector2.up;
        return moveDirection * MovementSpeed; ;
    }
}
