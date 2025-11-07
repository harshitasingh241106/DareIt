using UnityEngine;
using System.Collections.Generic;

public class Path : MonoBehaviour
{
    public List<Tile> tiles = new List<Tile>();

    void Awake()
    {
        tiles.Clear();
        int index = 0;

        foreach (Transform child in transform)
        {
            Tile tile = child.GetComponent<Tile>();
            if (tile != null)
            {
                tile.tileIndex = index;   // auto-number each tile
                tiles.Add(tile);
                index++;
            }
        }

        // Optionally link the next tile
        for (int i = 0; i < tiles.Count - 1; i++)
        {
            tiles[i].nextTile = tiles[i + 1];
        }
    }
}
