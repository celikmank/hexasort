
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class HexSpawner : MonoBehaviour
{
    [FormerlySerializedAs("_spawnPoints")] [SerializeField] private Transform[] spawnPoints;
    [FormerlySerializedAs("_tilesPrefabs")] [SerializeField] private Hexcell[] tilesPrefabs;
    [SerializeField] private HexHolder hexHolderPrefab;

    private List<HexHolder> _availableTilesPrefabs = new List<HexHolder>();

    public static HexSpawner instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }
    }
    private void Start()
    {
        SpawnTiles();
    }
    private void SpawnTiles()
    {
        if (tilesPrefabs.Length == 0)
        {
            Debug.LogError("No tiles prefabs available!");
            return;
        }
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            HexHolder obj = Instantiate(hexHolderPrefab, spawnPoints[i]);
            List<Hexcell> tilePool = new List<Hexcell>(tilesPrefabs);
            List<Hexcell> tempList = new List<Hexcell>();

            for (int j = 0; j < 3; j++)
            {
                var rnd = Random.Range(0, tilePool.Count);
                while (tempList.Contains(tilePool[rnd]))
                {
                    rnd = Random.Range(0, tilePool.Count);
                }
                tempList.Add(tilePool[rnd]);
            }

            for (int j = 0; j < tempList.Count; j++)
            {
                Hexcell tile = Instantiate(tempList[j], obj.transform);
                tile.transform.localPosition = Vector3.up * (j * 2.5f);
                obj.AddHex(tile);
            }
            _availableTilesPrefabs.Add(obj);
        }
    }
    public void RemoveFromAvailableTilesList(HexHolder obj)
    {
        if (_availableTilesPrefabs.Count != 0)
        {
            _availableTilesPrefabs.Remove(obj);
            if (_availableTilesPrefabs.Count == 0)
            {
                SpawnTiles();
            }

        }
    }   
}