using UnityEngine;
using UnityEngine.InputSystem;

public class EquippableSpell : MonoBehaviour
{
    // Fields
    [HideInInspector] public byte setIndex, spellIndex;
    [HideInInspector] public SpellSelectionManager managerScript;
    private bool dragging = false;
    
    // Monobehavior Methods
    private void Start()
    {
        GetComponent<SpriteRenderer>().sprite = GameSettings.Used.SpellSets[setIndex].spellsInSet[spellIndex].Icon;
    }
    private void Update()
    {
        if (dragging)
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            mousePos.z = Camera.main.orthographicSize * 2;
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
            
            transform.position = mouseWorldPos;
        }
    }
    
    // Methods
    private void OnMouseDown()
    {
        dragging = true;
            
        // Replace itself
        Instantiate(gameObject, transform.parent.transform);
    }
    
    private void OnMouseUp()
    {
        dragging = false;

        // Check to see if this can be placed in any slots
        managerScript.PlaceInSlot(this);
        Destroy(gameObject);
    }
}
