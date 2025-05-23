using UnityEngine;

public class SpellCollisions : MonoBehaviour
{
    
    // Fields
    private SpellInfoLogic spellInfoLogic;

    // Properties
    //SpellModule Module => spellInfoLogic.Module;

    // Methods
    /*private void Start()
    {
        spellInfoLogic = GetComponent<SpellInfoLogic>();

        if (Module.UsesCollider)
        {
            EnableCollider();
        }
    }*/
    /*private void EnableCollider()
    {
        PolygonCollider2D collider = GetComponent<PolygonCollider2D>();
        collider.enabled = true;
        collider.points = Module.ColliderPath;
    }*/
}
