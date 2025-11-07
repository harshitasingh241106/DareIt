using UnityEngine;
using System.Collections.Generic;

public class PathManager : MonoBehaviour
{
    [System.Serializable]
    public class Path
    {
        public string pathName;
        public Transform pathParent;
        [HideInInspector] public Transform[] tiles;

        public void InitializeTiles()
        {
            tiles = new Transform[pathParent.childCount];
            for (int i = 0; i < pathParent.childCount; i++)
                tiles[i] = pathParent.GetChild(i);
        }
    }

    public Path[] paths;

    void Start()
    {
        foreach (var path in paths)
            path.InitializeTiles();
    }

    // ✅ Collect all teleport tiles
    public List<(Transform tile, Transform pathParent)> GetAllTeleportationTiles()
    {
        List<(Transform, Transform)> teleportTiles = new List<(Transform, Transform)>();

        foreach (var path in paths)
        {
            foreach (var tile in path.tiles)
            {
                if (tile.CompareTag("Teleportation_tile"))
                    teleportTiles.Add((tile, path.pathParent));
            }
        }

        return teleportTiles;
    }
}
