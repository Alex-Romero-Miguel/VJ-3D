using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapCreator : MonoBehaviour
{
    public static MapCreator Instance { get; private set; }

    public GameObject[] tileTypes;

    private GameObject mapRoot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public List<GameObject> CreateMap(TextAsset mapFile, Vector3 origin, Transform parent = null)
    {
        // Si ya había un mapa creado → destruirlo
        DestroyMap();

        List<GameObject> spawnedTiles = new List<GameObject>();

        if (mapFile == null)
        {
            Debug.LogError("MapCreator: archivo de mapa es null");
            return null;
        }

        char[] seps = { ' ', '\n', '\r' };
        string[] snums = mapFile.text.Split(seps, StringSplitOptions.RemoveEmptyEntries);

        int[] nums = new int[snums.Length];
        for (int i = 0; i < snums.Length; i++)
            nums[i] = int.Parse(snums[i]);

        int sizeX = nums[0];
        int sizeZ = nums[1];

        // Crear raíz del mapa
        mapRoot = new GameObject("Map_" + mapFile.name);
        if (parent != null)
            mapRoot.transform.parent = parent;

        int index = 2;
        int sizeZminus1 = sizeZ - 1;
        for (int z = 0; z < sizeZ; z++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                int tileId = nums[index++];

                if (tileId == 0)
                    continue; // espacio vacío

                if (tileId < 1 || tileId > tileTypes.Length)
                {
                    Debug.LogWarning($"MapCreator: tileId {tileId} fuera de rango.");
                    continue;
                }

                GameObject prefab = tileTypes[tileId-1];
                if (prefab == null) continue;

                Vector3 pos = origin + new Vector3(x, 0f, sizeZminus1 - z);
                GameObject tile = Instantiate(prefab, pos, Quaternion.identity);
                tile.transform.parent = mapRoot.transform;
                
                spawnedTiles.Add(tile);
            }
        }
        return spawnedTiles;
    }

    public void DestroyMap()
    {
        if (mapRoot != null)
        {
            StopAllTileAnimations();
            
            Destroy(mapRoot);
            mapRoot = null;
        }
    }

    private void StopAllTileAnimations()
    {
        if (mapRoot == null) return;
        foreach (Transform t in mapRoot.transform)
        {
            if (t == null) continue;
            var ta = t.GetComponent<TileAnimator>();
            if (ta != null) ta.StopAllCoroutines();
        }
    }
}