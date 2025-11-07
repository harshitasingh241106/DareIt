using UnityEngine;

public class EnemyPieceController : MonoBehaviour
{
    [Header("Path Settings")]
    public Transform currentPath;
    public int currentIndex;
    public bool isOnBoard;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;

    private bool isMoving = false;

    public void PlaceOnStartTile(Transform startTile, Transform pathParent)
    {
        currentPath = pathParent;
        currentIndex = 0;
        transform.position = startTile.position;
        isOnBoard = true;
    }

    public void MoveEnemy(int steps, bool moveBackward = false)
    {
        if (!isOnBoard || isMoving) return;
        StartCoroutine(MoveStepByStep(steps, moveBackward));
    }

    private System.Collections.IEnumerator MoveStepByStep(int steps, bool moveBackward)
    {
        isMoving = true;

        for (int i = 0; i < steps; i++)
        {
            int nextIndex = moveBackward ? currentIndex - 1 : currentIndex + 1;

            // Boundary check
            if (nextIndex < 0 || nextIndex >= currentPath.childCount)
                break;

            Transform nextTile = currentPath.GetChild(nextIndex);

            // Smooth movement
            while (Vector3.Distance(transform.position, nextTile.position) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, nextTile.position, moveSpeed * Time.deltaTime);
                yield return null;
            }

            currentIndex = nextIndex;
            yield return new WaitForSeconds(0.1f);
        }

        isMoving = false;
        yield break;
    }
}
