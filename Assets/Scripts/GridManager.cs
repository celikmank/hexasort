
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HexCell;
using DG.Tweening;

public class GridManager : MonoBehaviour
{
    public static GridManager ınstance;
    public int width = 6;
    public int height = 6;

    public Color defaultColor = Color.white;
    public Color touchedColor = Color.magenta;

    public HexCell cellPrefab;
    public ColorType[] colorTypes;
    HexCell[] _cells;

    private bool _isProcessing = false;

    public bool IsProcessing
    {
        get => _isProcessing;
        set => _isProcessing = value;
    }

    void Awake()
    {
        ınstance = this;
        _cells = new HexCell[height * width];

        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }

    public HexCell pickedCell;
    [SerializeField] float distanceCheck;

    public HexCell FindCell(GameObject obj)
    {
        foreach (var cell in _cells)
        {
            if (Vector3.Distance(obj.transform.position, cell.transform.position) < distanceCheck)
            {
                if (pickedCell != null && pickedCell != cell)
                {
                    pickedCell.meshRenderer.material.color = defaultColor;
                }

                pickedCell = cell;
                pickedCell.meshRenderer.material.color = touchedColor;
                return pickedCell;
            }
        }

        if (pickedCell != null)
        {
            pickedCell.meshRenderer.material.color = defaultColor;
            pickedCell = null;
        }

        return pickedCell;
    }

    public HexCell FindCellAtPosition(Vector3 position)
    {
        foreach (HexCell cell in _cells)
        {
            if (cell.coord == position)
            {
                return cell;
            }
        }

        return null;
    }

    void CreateCell(int x, int z, int i)
    {
        Vector3 position;
        position.x = (x + z * 0.5f - z / 2) * (HexMetrics.İnnerRadius * 3f);
        position.y = 0f;
        position.z = z * (HexMetrics.OuterRadius * 1.5f);

        HexCell cell = _cells[i] = Instantiate(cellPrefab);
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.color = defaultColor;
        cell.coord = new Vector3(x, 0, z);
        cell.gameObject.name = "Cell" + cell.coord;
    }

    public void CheckNeighborsForColor(HexCell hexCell)
    {
        if (hexCell == null || hexCell.hexHolder == null || _isProcessing)
        {
            Debug.LogWarning("hexCell, hexCell.hexHolder is null or processing is in progress.");
            return;
        }

        StartCoroutine(CheckNeighborsForColorCoroutine(hexCell));
    }

    private IEnumerator CheckNeighborsForColorCoroutine(HexCell hexCell)
    {
        _isProcessing = true;

        List<HexCell> temp = new List<HexCell>();
        bool foundMatch;

        do
        {
            foundMatch = false;
            foreach (HexCell neighbour in hexCell.GetNeighbors())
            {
                if (neighbour == null || neighbour.hexHolder == null)
                {
                    continue;
                }

                Hexcell neighbourFirstHex = neighbour.hexHolder.HexPeek();
                Hexcell hexCellFirstHex = hexCell.hexHolder.HexPeek();

                if (neighbourFirstHex != null && hexCellFirstHex != null &&
                    neighbourFirstHex.color == hexCellFirstHex.color)
                {
                    foundMatch = true;

                    // Move the neighbor hex to the position with a delay
                    yield return neighbourFirstHex.transform
                        .DOMove(hexCellFirstHex.transform.position + Vector3.up * 2.5f, 0.5f)
                        .WaitForCompletion();

                    // Reparent and manage the hexes
                    neighbourFirstHex.transform.SetParent(hexCellFirstHex.transform.parent);
                    hexCell.hexHolder.AddHex(neighbour.hexHolder.RemoveHex());
                    temp.Add(neighbour);

                    if (neighbour.hexHolder.GetHex().Count == 0)
                    {
                        Debug.Log("Komşularda hex bulunamadı...", neighbour);
                        neighbour.occupationStatus = CellOccupationStatus.Unoccupied;
                    }

                    // Recursively check the neighbors
                    yield return StartCoroutine(CheckNeighborsForColorCoroutine(hexCell));
                }
            }
        } while (foundMatch);

        foreach (HexCell neighbour in temp)
        {
            if (neighbour != null)
            {
                Debug.Log("bu fonksiyona girdi");
                yield return StartCoroutine(CheckNeighborsForColorCoroutine(neighbour));
            }
        }
        //if (hexCell.hexHolder.GetHex().Count == 0)
        //{
        //    Destroy(hexCell.hexHolder.gameObject);
        //}

        _isProcessing = false;
        DestroyMatchingHexes(hexCell);
    }
    private void DestroyMatchingHexes(HexCell hexCell)
    {
        if (hexCell.hexHolder.GetHex().Count < 3)
        {
            return;
        }

        ColorType color = hexCell.hexHolder.HexPeek().color;
        List<Hexcell> hexesToRemove = new List<Hexcell>();

        foreach (Hexcell hex in hexCell.hexHolder.GetHex())
        {
            if (hex != null && hex.color == color)
            {
                hexesToRemove.Add(hex);
            }
        }

        if (hexesToRemove.Count >= 6)
        {
            StartCoroutine(RemoveHexesWithDelay(hexesToRemove, hexCell));
        }
    }
    private IEnumerator RemoveHexesWithDelay(List<Hexcell> hexesToRemove, HexCell hexCell)
    {
        List<HexCell> affectedCells = new List<HexCell>();
        _isProcessing = true;

        foreach (Hexcell hex in hexesToRemove)
        {
            if (hex != null)
            {
                // Shrink the hex before destroying it
                yield return hex.transform
                    .DOScale(Vector3.zero, 0.5f)  // Shrinks the hex over 0.5 seconds
                    .SetEase(Ease.InBack)         // Uses a smooth easing function
                    .WaitForCompletion();

                Destroy(hex.gameObject);
                hexCell.hexHolder.RemoveHex();

                yield return new WaitForSeconds(0.1f); // Add a slight delay between each destruction
            }
        }

        foreach (HexCell neighbor in hexCell.GetNeighbors())
        {
            if (neighbor != null)
            {
                affectedCells.Add(neighbor);
            }
        }

        if (hexCell.hexHolder != null && hexCell.hexHolder.GetHex().Count == 0)
        {
            hexCell.occupationStatus = CellOccupationStatus.Unoccupied;
        }

        _isProcessing = false;

        foreach (HexCell cell in affectedCells)
        {
            if (cell != null)
            {
                CheckNeighborsForColor(cell);
            }
        }
    }

}

//    private void DestroyMatchingHexes(HexCell hexCell)
//    {
//        if (hexCell.hexHolder.GetHex().Count < 3)
//        {
//            return;
//        }

//        ColorType color = hexCell.hexHolder.HexPeek().color;
//        List<Hexcell> hexesToRemove = new List<Hexcell>();

//        foreach (Hexcell hex in hexCell.hexHolder.GetHex())
//        {
//            if (hex != null && hex.color == color)
//            {
//                hexesToRemove.Add(hex);
//            }
//        }

//        if (hexesToRemove.Count >= 6)
//        {
//            StartCoroutine(RemoveHexesWithDelay(hexesToRemove, hexCell));
//        }
//    }
//    private IEnumerator RemoveHexesWithDelay(List<Hexcell> hexesToRemove, HexCell hexCell)
//    {
//        List<HexCell> affectedCells = new List<HexCell>();
//        _isProcessing = true;

//        foreach (Hexcell hex in hexesToRemove)
//        {
//            if (hex != null)
//            {
//                Destroy(hex.gameObject);
//                hexCell.hexHolder.RemoveHex();
//                yield return new WaitForSeconds(0.5f); // Her yok etme işlemi arasında 1 saniye bekleme
//            }
//        }

//        foreach (HexCell neighbor in hexCell.GetNeighbors())
//        {
//            if (neighbor != null)
//            {
//                affectedCells.Add(neighbor);
//            }
//        }

//        if (hexCell.hexHolder != null && hexCell.hexHolder.GetHex().Count == 0)
//        {
//            hexCell.occupationStatus = CellOccupationStatus.Unoccupied;
//        }

//        _isProcessing = false;
//        foreach (HexCell cell in affectedCells)
//        {
//            if (cell != null)
//            {
//                CheckNeighborsForColor(cell);
//            }
//        }
//    }
//}


//     public void CheckNeighborsForColor(HexCell hexCell)
//     {
//         if (hexCell == null || hexCell.hexHolder == null || _isProcessing)
//         {
//             Debug.LogWarning("hexCell, hexCell.hexHolder is null or processing is in progress.");
//             return;
//         }
//
//         List<HexCell> temp = new();
//         bool foundMatch;
//         do
//         {
//             foundMatch = false;
//             foreach (HexCell neighbour in hexCell.GetNeighbors())
//             {
//                 if (neighbour == null || neighbour.hexHolder == null)
//                 {
//                     continue;
//                 }
//
//                 Hexcell neighbourFirstHex = neighbour.hexHolder.HexPeek();
//                 Hexcell hexCellFirstHex = hexCell.hexHolder.HexPeek();
//
//                 if (neighbourFirstHex != null && hexCellFirstHex != null)
//                 {
//                     if (neighbourFirstHex.color == hexCellFirstHex.color)
//                     {
//                         neighbourFirstHex.transform
//                             .DOMove(hexCellFirstHex.transform.position + Vector3.up * 2.5f, 0.5f)
//                             .OnComplete(() =>
//                             {
//                                 neighbourFirstHex.transform.parent = hexCellFirstHex.transform.parent;
//                                 hexCell.hexHolder.AddHex(neighbour.hexHolder.RemoveHex());
//                                 temp.Add(neighbour);
//
//                                 if (neighbour.hexHolder.GetHex().Count == 0)
//                                 {
//                                     Debug.Log("Komşularda hex bulunamadı...", neighbour);
//                                     neighbour.occupationStatus = CellOccupationStatus.Unoccupied;
//                                 }
//
//                                 CheckNeighborsForColor(hexCell);
//                                
//                             });
//                         Debug.Log();
//                         /*
//                         neighbourFirstHex.transform.parent = hexCellFirstHex.transform.parent;
//                         hexCell.hexHolder.AddHex(neighbour.hexHolder.RemoveHex());
//                         temp.Add(neighbour);
//                         foundMatch = true;
//
//                         if (neighbour.hexHolder.GetHex().Count == 0)
//                         {
//                             Debug.Log("Komşularda hex bulunamadı...", neighbour);
//                             neighbour.occupationStatus = CellOccupationStatus.Unoccupied;
//                         }
//
//                     }
//                     */
//                         if (hexCell.hexHolder.GetHex().Count == 0)
//                         {
//                             Destroy(hexCell.hexHolder.gameObject);
//                         }
//                     }
//                 }
//             }
//         } while (foundMatch);
//         Debug.Log(());
//         foreach (HexCell neighbour in temp)
//         {
//             if (neighbour != null)
//             {
//                 Debug.Log("bu fonksiyona girdi");
//                 CheckNeighborsForColor(neighbour);
//             }
//         }
//         // Debug.Log("DestroyMatchingHexes: " + hexCell.name, hexCell);
//         // DestroyMatchingHexes(hexCell);
//     }

