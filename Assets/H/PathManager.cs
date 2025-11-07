using UnityEngine;

[System.Serializable]
public class Path
{
    public string pathName;
    public Transform pathParent;
    [HideInInspector]
    public Transform[] tiles;

    public void InitializeTiles()
    {
        tiles = new Transform[pathParent.childCount];
        for (int i = 0; i < pathParent.childCount; i++)
            tiles[i] = pathParent.GetChild(i);
    }
}

public class PathManager : MonoBehaviour
{
    public Path[] paths;

    void Awake()
    {
        foreach (var path in paths)
            path.InitializeTiles();
    }
}
