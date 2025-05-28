using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Spell Movement/Move By Direction")]
public class MoveByDirection : SpellMovement
{
    public override Vector2 Move(float angle, byte module)
    {
        float offsetAngle = angle + RotationOffset;

        Vector2 moveDirection = Quaternion.Euler(0, 0, offsetAngle) * Vector2.up;
        return moveDirection * MovementSpeed;
    }
}
