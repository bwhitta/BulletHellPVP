using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EquippableSpell : MonoBehaviour/*, IBeginDragHandler, IDragHandler, IEndDragHandler*/
{
    // Fields
    [HideInInspector] public byte setIndex, spellIndex;
    [HideInInspector] public SpellSelectionManager managerScript;
    
    // Monobehavior Methods
    private void Start()
    {
        GetComponent<Image>().sprite = GameSettings.Used.SpellSets[setIndex].spellsInSet[spellIndex].Icon;
    }

    // Methods

    public void OnDragStart()
    {
        // Replace itself
        Instantiate(gameObject, transform.parent.transform);
    }
    public void DragUpdate(BaseEventData eventData)
    {
        /*Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos.z = Camera.main.orthographicSize * 2;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

        transform.position = mouseWorldPos;*/

        var pointerEventData = (PointerEventData)eventData;
        transform.position = pointerEventData.position; //maybe need to use recttransform
    }
    public void OnDragEnd()
    {
        // Check to see if this can be placed in any slots
        managerScript.PlaceInSlot(this);
        Destroy(gameObject);
    }

    /*public void OnBeginDrag(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnDrag(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }*/
}
