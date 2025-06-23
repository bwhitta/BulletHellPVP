using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class PrefabActionStuff : NetworkBehaviour, IPointerClickHandler
{
    void Start()
    {
        if (IsOwnedByServer)
        {
            GetComponent<SpriteRenderer>().color = Color.darkOrchid;
        }
        else
        {
            GetComponent<SpriteRenderer>().color = Color.hotPink;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"pointer click event");
        PointerClickedServerRpc();
    }

    [ServerRpc]
    private void PointerClickedServerRpc()
    {
        PointerClickedClientRpc();
    }
    [ClientRpc]
    private void PointerClickedClientRpc()
    {
        Debug.Log($"CLICKED!");
    }

    
}