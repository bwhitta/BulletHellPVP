using UnityEngine;

public abstract class SpellMovement : ScriptableObject
{
    // Fields
    public float MovementSpeed;
    public float RotationOffset;
    
    // Methods
    public abstract Vector2 Move(float angle, byte moduleObjectIndex);
}
