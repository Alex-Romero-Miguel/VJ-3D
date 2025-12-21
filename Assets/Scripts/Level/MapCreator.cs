using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapCreator : MonoBehaviour
{
    public static MapCreator Instance { get; private set; }

    public GameObject[] tileTypes;

    private GameObject mapRoot;

    public Vector3 PlayerStartWorldPos { get; private set; }

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

    private class ParsedTile
    {
        public int id;        // ID del prefab (1–n)
        public int canal_id;   // variant opcional (0–n)
        public string extra;  // text opcional ("L", "R", etc.)
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

        char[] seps = { ' ', '\n', '\r', '\t' };
        string[] tokens = mapFile.text.Split(seps, StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length < 2)
        {
            Debug.LogError("MapCreator: archivo no válido.");
            return null;
        }

        // Llegim les dimensions
        int sizeX = int.Parse(tokens[0]);
        int sizeZ = int.Parse(tokens[1]);

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
                if (index >= tokens.Length)
                {
                    Debug.LogWarning("Mapa incompleto o mal formateado.");
                    break;
                }

                string token = tokens[index++];

                if (token == "0")
                    continue;

                ParsedTile parsed = ParseToken(token);

                if (parsed.id < 1 || parsed.id > tileTypes.Length)
                {
                    Debug.LogWarning($"ID {parsed.id} fuera de rango. Token: {token}");
                    continue;
                }

                GameObject prefab = tileTypes[parsed.id - 1];
                if (prefab == null) continue;

                Vector3 pos = origin + new Vector3(x, 0f, sizeZminus1 - z);
                GameObject tile = Instantiate(prefab, pos, Quaternion.identity, mapRoot.transform);

                // Si el tile té configuració
                ITileConfigurable cfg = tile.GetComponent<ITileConfigurable>();
                if (cfg != null)
                    cfg.Configure(parsed.canal_id, parsed.extra);

                // si es un boton split
                SplitButton splitBtn = tile.GetComponent<SplitButton>();
                if (splitBtn != null)
                {
                    int[] p = Array.ConvertAll(parsed.extra.Split(','), int.Parse);

                    //Vector3 localPosA = new Vector3(p[1], 0.1f, sizeZminus1 - p[0]);
                    //Vector3 posA = mapRoot.transform.TransformPoint(localPosA);

                    //Vector3 localPosB = new Vector3(p[3], 0.1f, sizeZminus1 - p[2]);
                    //Vector3 posB = mapRoot.transform.TransformPoint(localPosB);

                    Vector3 posA = origin + new Vector3(p[1], 0f, sizeZminus1 - p[0]);
                    Vector3 posB = origin + new Vector3(p[3], 0f, sizeZminus1 - p[2]);

                    splitBtn.SetSplitPositions(posA, posB);
                }


                spawnedTiles.Add(tile);
            }
        }


        // Leer posicion inicial del jugador
        PlayerStartWorldPos = new Vector3(5, 1f, sizeZminus1 - 10);
        if (tokens.Length >= index + 2)
        {
            int pz = int.Parse(tokens[index++]); // fila (z en el mapa)
            int px = int.Parse(tokens[index++]); // columna (x)

            //Vector3 spawn = origin + new Vector3(px, 1f, sizeZminus1 - pz);
            PlayerStartWorldPos = new Vector3(px, 1f, sizeZminus1 - pz);
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

    private ParsedTile ParseToken(string token)
    {
        // Formats suportats:
        // "3"
        // "4:0"  puente
        // "6:1:L" boton puente
        // "7:2,1,3,1"  boton split

        ParsedTile pt = new ParsedTile();

        string[] p = token.Split(':');

        pt.id = int.Parse(p[0]);
        pt.canal_id = 0;
        pt.extra = null;

        //// Variant opcional
        //if (p.Length > 1)
        //    pt.canal_id = int.Parse(p[1]);

        //// Extra opcional
        //if (p.Length > 2)
        //    pt.extra = p[2];

        if (p.Length > 1)
        {
            bool isChannelNumber = int.TryParse(p[1], out int cId);

            if (isChannelNumber)
            {
                pt.canal_id = cId;
                if (p.Length > 2)
                    pt.extra = pt.extra = p[2];
            }
            else
            {
                pt.extra = p[1];
            }
        }


        return pt;
    }
}