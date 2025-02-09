using UnityEngine;

public class SpellCollisions : MonoBehaviour
{
    // Fields
    private SpellModuleBehavior spellModuleBehavior; // rename as soon as I rename the spellModuleBehavior script;

    // Properties
    SpellData.Module Module => spellModuleBehavior.Module;

    // Methods
    private void Start()
    {
        spellModuleBehavior = GetComponent<SpellModuleBehavior>();

        if (Module.UsesCollider)
        {
            EnableCollider();
        }
    }
    private void EnableCollider()
    {
        PolygonCollider2D collider = GetComponent<PolygonCollider2D>();
        collider.enabled = true;
        collider.points = Module.ColliderPath;
    }
}
