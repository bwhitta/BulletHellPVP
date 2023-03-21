using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class EquippableSpell : MonoBehaviour
{
    [HideInInspector] public SpellData spellData;
    private bool dragging = false;
    [HideInInspector] public bool sucessfullyPlaced = false;

    private Vector3 MouseWorldPosition
    {
        get
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            //mousePos.z = Camera.main.nearClipPlane;
            mousePos.z = Camera.main.orthographicSize * 2;
            //mousePos.z = 0;
            Vector3 Worldpos = Camera.main.ScreenToWorldPoint(mousePos);
            return Worldpos;
        }
    }
    private SpriteRenderer spriteRenderer;

    // Object references
    private static SpellSelectionManager spellSelectionManager;
    private void Start()
    {
        // Set references
        if(spellSelectionManager == null)
        {
            spellSelectionManager = transform.parent.GetComponent<SpellSelectionManager>();
        }
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = spellData.Icon;
    }
    private void Update()
    {
        if (dragging)
        {
            transform.position = MouseWorldPosition;
        }
    }
    
    private void OnMouseDown()
    {
        if (!sucessfullyPlaced)
        {
            ReplaceSelf();
            dragging = true;
        }
    }
    private void OnMouseUp()
    {
        dragging = false;

        // Check if it was dropped on a slot
        Debug.Log("Mouse up");
        CheckSlots();

    }
    private void ReplaceSelf()
    {
        Instantiate(gameObject, spellSelectionManager.transform);
    }

    private void CheckSlots()
    {
        Debug.Log("Checking slots.");
        sucessfullyPlaced = spellSelectionManager.PlaceInSlot(this);
        if (sucessfullyPlaced)
        {
            transform.parent = spellSelectionManager.equippedSpellArea.transform;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}