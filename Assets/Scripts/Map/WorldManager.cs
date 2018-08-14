using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldManager : MonoBehaviour
{
    public Vector2Int mapBounds = new Vector2Int(49, 48);//bounds of the map in tile coords
    float snowGrowSpeed = 0.001f;//between 0 and 1 - the percentage of edge snow tiles which grow each frame// 0.35 reconmended
    HashSet<Vector2Int> snowTilePositions;
    public Dictionary<Vector2Int, SnowTile> snowEffects;
    public Dictionary<Vector2Int, Heater> heaters;


    public static WorldManager singleton;
    System.Random rand;
    public Tilemap tilemap;

    TileBase[] tiles;
    public GameObject destroyedBlockPrefab;
    public GameObject snowTilePrefab;
    public GameObject selectedBlockPrefab;
    public List<GameObject> players;
    public Tile[] currentlySelectedTiles; //the tiles the players are selecting respectively

    public static int noPlayers = 1;

    public Transform healthBarsParent;
    //Sounds
    public AudioSource towerPlacement;
    
    // Use this for initialization
    void Start()
    {
        singleton = this;

        noPlayers = GameInfo.numPlayers;

        rand = new System.Random();
        snowTilePositions = new HashSet<Vector2Int>();
        snowEffects = new Dictionary<Vector2Int, SnowTile>();
        currentlySelectedTiles = new Tile[noPlayers];
        heaters = new Dictionary<Vector2Int, Heater>();

        placeSnowExterior();

    }



    private void placeSnowExterior()
    {
        for (int x = 0; x < mapBounds.x; x++)
        {
            for (int y = 0; y < mapBounds.y; y++)
            {
                if (x == 0 || y == 0 || x == mapBounds.x - 1 || y == mapBounds.y - 1)
                {
                    spawnSnowTile(new Vector2Int(x, y));
                }
            }
        }
    }

    private void Update()
    {
        growSnow();
    }

    private void growSnow()
    {
        ArrayList edgeSnowTiles = new ArrayList();//a list of snow tiles which may be added
        HashSet<Vector2Int> uniqueEdgeSnowTiles = new HashSet<Vector2Int>();
        foreach (Vector2Int pos in snowTilePositions)
        {
            if (!snowEffects[pos].isInAnchorPosition())
            {
                continue;
            }
            //if(snowTiles.Contains())


            HashSet<Vector2Int> neighbours = getCellNeighboursIterator(pos);
            foreach (Vector2Int nei in neighbours)
            {
                if (!snowTilePositions.Contains(nei))
                {
                    if (!uniqueEdgeSnowTiles.Contains(nei))
                    {
                        edgeSnowTiles.Add(nei);
                        uniqueEdgeSnowTiles.Add(nei);
                    }
                }
            }
        }


        int newSnowTiles = Math.Max(1, (int)Math.Round(snowGrowSpeed * edgeSnowTiles.Count * Time.deltaTime));
        //Debug.Log(snowGrowSpeed * edgeSnowTiles.Count * Time.deltaTime + " ; " + newSnowTiles + " ; " + edgeSnowTiles.Count) ;
        if (edgeSnowTiles.Count == 0) return;
        for (int i = 0; i < newSnowTiles; i++)
        {
            //int id = rand.Next(0, edgeSnowTiles.Count);
            int id = UnityEngine.Random.Range(0, edgeSnowTiles.Count);
            spawnSnowTile((Vector2Int)edgeSnowTiles[id]);
            edgeSnowTiles.RemoveAt(id);
        }
    }

    internal GameObject placeObjectOnSelectedTile(GameObject obj, int playerID, bool instantiate)
    {
        GameObject ret;
        bool heater = obj.GetComponent<Heater>() != null;
        if (heater)
        {
            if (heaters.ContainsKey(currentlySelectedTiles[playerID].attachedTileIndex))
            {//there is already a heater here
                Debug.Log("trying to place a heater on top of existing heater");
                return null;
            }
            bool neighbourFull = false;
            HashSet<Vector2Int> neighbours = getCellNeighboursIterator(currentlySelectedTiles[playerID].attachedTileIndex);
            foreach (Vector2Int nei in neighbours)
            {
                if (heaters.ContainsKey(nei))
                {
                    neighbourFull = true;
                }
            }
            if (neighbourFull)
            {
                Debug.Log("trying to place a heater next to another one");
                return null;
            }
        }
        if (instantiate)
        {
            GameObject inst = placeObject(currentlySelectedTiles[playerID].attachedTileIndex, obj);
            ret = inst;
        }
        else
        {
            //Debug.Log("replacing an object: " + obj.name);
            Tile tileObj = obj.GetComponent<Tile>();
            if (tileObj == null)
            {
                Debug.LogError("trying to place object on map without tile component");
            }
            tileObj.AnchorPosition = tilemap.CellToWorld(new Vector3Int(tilemap.origin.x + currentlySelectedTiles[playerID].attachedTileIndex.x, tilemap.origin.y + currentlySelectedTiles[playerID].attachedTileIndex.y, 1));
            tileObj.attachedTileIndex = currentlySelectedTiles[playerID].attachedTileIndex;
            ret = obj;
            tileObj.resetPosition();
            
            // Play effect when replaced
            ((Heater)tileObj).heaterPlaceEffect.Play();
        }

        if (heater)
        {
            //Debug.Log("placed heater - had snow: " + snowEffects.ContainsKey(currentlySelectedTiles[playerID].attachedTileIndex));
            heaters.Add(currentlySelectedTiles[playerID].attachedTileIndex, ret.GetComponent<Heater>());
            ret.transform.parent = transform.GetChild(3);
            if (snowEffects.ContainsKey(currentlySelectedTiles[playerID].attachedTileIndex))
            {
                destroySnowTile(currentlySelectedTiles[playerID].attachedTileIndex);
            }

            //Sound
            towerPlacement.Play();
        }
        return ret;


    }

    internal void removeHeaterAt(Vector2Int attachedTileIndex)
    {
        heaters.Remove(attachedTileIndex);
    }

    public GameObject placeObject(Vector2Int pos, GameObject obj)
    {
        GameObject tile =
         Instantiate
        (
            obj,
            tilemap.CellToWorld(new Vector3Int(tilemap.origin.x + pos.x, tilemap.origin.y + pos.y, 1)),
            Quaternion.identity
        );
        tile.transform.parent = transform.GetChild(2);
        Tile tileObj = tile.GetComponent<Tile>();
        if (tileObj == null)
        {
            Debug.LogError("trying to place object on map without tile component");
        }
        tileObj.AnchorPosition = tilemap.CellToWorld(new Vector3Int(tilemap.origin.x + pos.x, tilemap.origin.y + pos.y, 1));
        tileObj.attachedTileIndex = pos;

        return tile;
    }

    private void spawnSnowTile(Vector2Int pos)
    {
        GameObject effect = placeObject(pos, snowTilePrefab);
        snowTilePositions.Add(pos);
        SnowTile snow = effect.GetComponent<SnowTile>();
        snowEffects.Add(pos, snow);
    }

    public void destroySnowTile(Vector2Int pos)
    {
        if (snowTilePositions.Contains(pos))
        {
            snowTilePositions.Remove(pos);
            Destroy(snowEffects[pos].gameObject, 0.3f);
            snowEffects.Remove(pos);
        }
    }


    internal Building SelectCell(Vector2Int pos, int playerNumber, Color playerColour)
    {
        if (currentlySelectedTiles[playerNumber] == null || currentlySelectedTiles[playerNumber].attachedTileIndex != pos)
        {
            //has selected a different tile
            GameObject selectedTile = placeObject(pos, selectedBlockPrefab);

            selectedTile.GetComponent<SpriteRenderer>().color = new Color(playerColour.r, playerColour.g, playerColour.b, selectedTile.GetComponent<SpriteRenderer>().color.a);
            if (currentlySelectedTiles[playerNumber] != null)
            {
                Destroy(currentlySelectedTiles[playerNumber].gameObject);
            }
            currentlySelectedTiles[playerNumber] = selectedTile.GetComponent<Tile>();
        }
        if (heaters.ContainsKey(currentlySelectedTiles[playerNumber].attachedTileIndex))
        {            
            return heaters[currentlySelectedTiles[playerNumber].attachedTileIndex];
        }

        return null;
    }

    public HashSet<Vector2Int> getCellNeighboursIterator(Vector2Int pos)
    {
        HashSet<Vector2Int> iter = new HashSet<Vector2Int>();
        for (int x = pos.x - 1; x < pos.x + 2; x++)
        {
            for (int y = pos.y - 1; y < pos.y + 2; y++)
            {
                if (!(x == pos.x && y == pos.y))
                {//doesnt recreate itself
                    if (x != pos.x + (pos.y % 2 == 0 ? 1 : -1) || !(y == pos.y - 1 || y == pos.y + 1))
                    {//is a valid neighbour
                        if (x >= 0 && y >= 0 && x <= mapBounds.x && y <= mapBounds.y)
                        {
                            iter.Add(new Vector2Int(x, y));
                        }
                    }
                }
            }
        }
        return iter;
    }

    public bool checkIfInSnow(Vector3 pos)
    {
        Vector3Int cellPos = WorldManager.singleton.tilemap.WorldToCell(transform.position);
        cellPos.x -= WorldManager.singleton.tilemap.origin.x;
        cellPos.y -= WorldManager.singleton.tilemap.origin.y;
        //Debug.Log("player on tile: " + cellPos + " selected tile: " + WorldManager.singleton.currentlySelectedTiles[playerNumber].attachedTileIndex);
        if (WorldManager.singleton.snowEffects.ContainsKey(new Vector2Int(cellPos.x, cellPos.y)) && WorldManager.singleton.snowEffects[new Vector2Int(cellPos.x, cellPos.y)].isInAnchorPosition())
        {
            return true;
        }
        else {
            return false;
        }
    }

}