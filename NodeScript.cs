using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class NodeScript : MonoBehaviour, IDropHandler
{
    public int AttachedCoinNumbering { get; set; }

    // Now event System Control is left for Nodes
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;
        FindObjectOfType<AudioManager>().Play("Magnet");
        var position = transform.position;
        eventData.pointerDrag.transform.position = new Vector3(position.x, position.y, eventData.pointerDrag.transform.position.z);
        
        eventData.pointerDrag.GetComponent<MoveTheCoins>().AttachedNode = gameObject;

        // Change the Sorting Layer
        eventData.pointerDrag.GetComponent<SpriteRenderer>().sortingLayerName = $"FilledNode";
        eventData.pointerDrag.transform.GetChild(0).GetComponent<SortingGroup>().sortingLayerName = $"LowerText";

        AttachedCoinNumbering = eventData.pointerDrag.GetComponent<NumberingScript>().Numbering;
        GetComponent<NodeScript>().enabled = false;
    }
}