using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static HexCell;

public class HexHolder: MonoBehaviour
{
    Vector3 _offset;
   [SerializeField] private HexCell currentCell;
   private Stack<Hexcell> _hexs = new Stack<Hexcell>();
   
   
    public void AddHex(Hexcell hex)//Hex
    {
       _hexs.Push(hex);     
    }
    public Hexcell RemoveHex()
    {
        if (_hexs.Count > 0)
        {
            return _hexs.Pop();
        }
        else
        {
            Debug.Log("Hex stack is empty. Cannot remove hex.");
            return null;
        }
    }

    public Hexcell HexPeek()
    {
        if (_hexs.Count > 0)
        {
            return _hexs.Peek();
        }
        else
        {
            Debug.Log("Hex stack is empty.");
            return null;
        }
    }
    public Stack<Hexcell> GetHex() {
        return _hexs;
    }
    
    void Start() {
        _offset = transform.position - MouseWorldPosition();
    }

    void OnMouseDown() {
        // if (GridManager.ınstance.IsProcessing)
        // {
        //     return;
        // }
        _offset = transform.position - MouseWorldPosition();
        GetComponent<Collider>().enabled = false;
    }

    void OnMouseDrag() {
        // if (GridManager.ınstance.IsProcessing)
        // {
        //     return;
        // }
        transform.position = MouseWorldPosition() + _offset;
        currentCell = GridManager.ınstance.FindCell(gameObject);
    }

    void OnMouseUp()
    {

        if (currentCell != null && currentCell.occupationStatus == CellOccupationStatus.Unoccupied)
        {
            currentCell.hexHolder = this;
            currentCell.PlaceObject(this);
            transform.parent = currentCell.transform;
            HexSpawner.instance.RemoveFromAvailableTilesList(this);

        }
        else
        {
            transform.position = transform.parent.position; // Or set your original position here
            transform.GetComponent<Collider>().enabled = true;
            currentCell = null; // Clear current cell reference
        }

    }

    Vector3 MouseWorldPosition()
    {
        var mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        var a = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        a.y = 0;
        return a;
    }
  
}
