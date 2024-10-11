using System.Collections.Generic;
using UnityEngine;
using static GridManager;

public class HexCell : MonoBehaviour
{
     void Start()
    {
        FindNeighbors();
    }

    [SerializeField] Vector3 ofset = Vector3.up * 2;
    public enum CellOccupationStatus
    {
        Unoccupied,
        Occupied
    }

    public Vector3 coord;
    [SerializeField] private List<HexCell> neighbors;
    public CellOccupationStatus occupationStatus = CellOccupationStatus.Unoccupied;
    public HexHolder occupiedObject;
    public Color color;
    public MeshRenderer meshRenderer;
    public HexHolder hexHolder;
    public Hexcell hexcolor;
    public ColorType colorType;
    
    public void PlaceObject(HexHolder obj) //hexholder
    {
        occupiedObject = obj;
        occupationStatus = CellOccupationStatus.Occupied;
        obj.transform.position = transform.position + ofset;
        color = ınstance.defaultColor;
        meshRenderer.material.color = color;

        GridManager.ınstance.CheckNeighborsForColor(this);
    }
    public List<HexCell> GetNeighbors()
    {
        return neighbors;
    }

    public void FindNeighbors()
    {
        neighbors = new List<HexCell>();

        bool isEvenColumn = coord.z % 2 == 0; // Check if current column is even

        // Define neighbor directions based on column parity
        Vector3[] directions = isEvenColumn ?
            new Vector3[] {
            new Vector3 (0, 0, 1),   //north-east
            new Vector3(1, 0, 0),    // east
            new Vector3(0, 0, -1),   // south-east
            new Vector3 (-1, 0, -1),  // south-west
            new Vector3 (-1, 0, 0),  // west
            new Vector3 (-1, 0, 1),  //north-west
            } :
            new Vector3[] {
            new Vector3(1, 0, 1),    // North-east
            new Vector3 (1, 0, 0),   // east
            new Vector3(1, 0, -1),   // south east
            new Vector3 (0, 0, -1),  // South-west
            new Vector3 (-1, 0, 0),  //  west
            new Vector3 (0, 0, 1),  // North-west//
            };

        foreach (Vector3 dir in directions)
        {
            Vector3 neighborPos = coord + dir;

            HexCell neighbor = GridManager.ınstance.FindCellAtPosition(neighborPos);
            if (neighbor != null)
            {
                neighbors.Add(neighbor);
            }
           


        }

    }
     
}
