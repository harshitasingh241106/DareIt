using UnityEngine;

public class Tile : MonoBehaviour
{
    public int tileIndex;
    public Tile nextTile; // for path chaining if needed
}
