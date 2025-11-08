using System.Collections;
using UnityEngine;

public class EnemyPieceController : PlayerPieceController
{
    public bool isBot = true;

    public void SpawnAtTile(Transform path, int index)
    {
        currentPath = path;
        currentIndex = index;
        transform.position = path.GetChild(index).position;
        isOnBoard = true;
    }

    public new IEnumerator MoveAlongPath(int steps, bool moveBackward)
    {
        yield return base.MoveAlongPath(steps, moveBackward);
    }
}
