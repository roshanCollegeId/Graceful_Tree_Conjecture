using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class MoveTheCoins : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public GameObject AttachedNode { get; set; }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.SetParent(GameAssets.GetTheInstance().coinsParent.transform);
        var anchoredPosition3D = GetComponent<RectTransform>().anchoredPosition3D;
        anchoredPosition3D = new Vector3(anchoredPosition3D.x, anchoredPosition3D.y, 0f);
        GetComponent<RectTransform>().anchoredPosition3D = anchoredPosition3D;

        // Block The Coins (Layer 7) Raycast
        GameAssets.GetTheInstance().camera.GetComponent<Physics2DRaycaster>().eventMask &= ~(1 << 7);
        // Get The Node Raycast
        GameAssets.GetTheInstance().camera.GetComponent<Physics2DRaycaster>().eventMask |= (1 << 6);
        
        // Change the Sorting Layer
        GetComponent<SpriteRenderer>().sortingLayerName = "ActiveCoin";
        GetComponent<SpriteRenderer>().sortingOrder = Int16.Parse(transform.GetChild(0).GetComponent<TextMeshPro>().text);
        transform.GetChild(0).GetComponent<SortingGroup>().sortingLayerName = $"ActiveCoin";
        transform.GetChild(0).GetComponent<SortingGroup>().sortingOrder = Int16.Parse(transform.GetChild(0).GetComponent<TextMeshPro>().text) + 1;
        
        if (AttachedNode != null)
        {
            AttachedNode.GetComponent<NodeScript>().enabled = true;
            AttachedNode.GetComponent<NodeScript>().AttachedCoinNumbering = 0;
            AttachedNode = null;
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!GameObject.FindGameObjectWithTag("ScrollTab").GetComponent<BoxCollider2D>()
            .OverlapPoint(transform.position))
        { Coin.ActivateModifiedCoinScale(transform, GameAssets.GetTheInstance().imagePrefab.transform); }
        
        else { Coin.ActivateOriginalCoinScale(transform); }

        GetComponent<RectTransform>().anchoredPosition += eventData.delta / GameAssets.GetTheInstance().canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsStillInsideTheScroll();
        GameAssets.GetTheInstance().camera.GetComponent<Physics2DRaycaster>().eventMask |= (1 << 7);
        GameAssets.GetTheInstance().camera.GetComponent<Physics2DRaycaster>().eventMask &= ~(1 << 6);
    }
    private void DropBackToTheScroll()
    {
        FindObjectOfType<AudioManager>().Play("Magnet");
        Transform parentTransform = GameObject.FindWithTag("ParentScroll").transform;
        
        // Change the Sorting Layer
        GetComponent<SpriteRenderer>().sortingLayerName = "Coin";
        transform.GetChild(0).GetComponent<SortingGroup>().sortingLayerID = SortingLayer.NameToID("LowerText");
        transform.GetChild(0).GetComponent<SortingGroup>().sortingOrder = 0;

        if (parentTransform.childCount == 0 || transform.position.x < parentTransform.GetChild(0).position.x)
        {
            transform.SetParent(parentTransform);
            transform.SetSiblingIndex(0);
            return;
        }
        if (parentTransform.childCount > 0 && transform.position.x >
            parentTransform.GetChild(parentTransform.childCount - 1).position.x)
        {
            transform.SetParent(parentTransform);
            transform.SetSiblingIndex(parentTransform.childCount - 1);
            return;
        }
        
        for (int i = 0; i < parentTransform.childCount - 1; i++)
        {
            if (parentTransform.childCount > 0 && parentTransform.GetChild(i).position.x < transform.position.x &&
                transform.position.x < parentTransform.GetChild(i + 1).position.x)
            {
                transform.SetParent(parentTransform);
                transform.SetSiblingIndex(i + 1);
                return;
            }
        }
    }
    private void IsStillInsideTheScroll()
    {
        BoxCollider2D scrollCollider = GameObject.FindGameObjectWithTag("ScrollTab").GetComponent<BoxCollider2D>();
        if (scrollCollider.OverlapPoint(transform.position))
        {
            DropBackToTheScroll();
        }
    }
}
