using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Path
{
    public string pathName;
    public Transform pathParent;
    [HideInInspector] public Transform[] tiles;
    public Transform startTile => tiles.Length > 0 ? tiles[0] : null;

    public void Initialize()
    {
        int count = pathParent.childCount;
        tiles = new Transform[count];
        for (int i = 0; i < count; i++)
            tiles[i] = pathParent.GetChild(i);
    }
}

public class PathManager : MonoBehaviour
{
    public static PathManager Instance;
    public List<Path> allPaths = new List<Path>();

    void Awake()
    {
        Instance = this;
        foreach (var p in allPaths)
            p.Initialize();
    }

    public Path GetPathFromStartTile(Transform startTile)
    {
        foreach (var path in allPaths)
            if (path.startTile == startTile)
                return path;
        return null;
    }
}
