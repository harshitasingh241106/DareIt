using UnityEngine;
using System.Collections;

public class PlayerPieceController : MonoBehaviour
{
    public static PlayerPieceController selectedPiece;

    public bool isOnBoard = false;
    public Transform currentPath;
    public int currentIndex = 0;
    public float moveSpeed = 3f;
    public string currentPathName;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            Debug.DrawRay(mousePos, Vector3.forward * 10, Color.red, 1f);
            Debug.Log($"Clicked at {mousePos}");

            if (hit.collider != null)
            {
                Debug.Log($"Hit object: {hit.collider.name}");
                if (hit.collider.gameObject == gameObject)
                {
                    selectedPiece = this;
                    Debug.Log($"✅ Selected piece: {name}");
                }
            }
            else
            {
                Debug.Log("❌ No collider hit!");
            }
        }
    }


    public void PlaceOnStartTile(Transform startTile, Transform pathHolder)
    {
        transform.position = startTile.position;
        currentPath = pathHolder;
        currentPathName = pathHolder.name;
        isOnBoard = true;
        currentIndex = 0;
    }

    public void MovePiece(int steps)
    {
        if (!isOnBoard || currentPath == null) return;
        StartCoroutine(MoveAlongPath(steps));
    }

    private IEnumerator MoveAlongPath(int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            if (currentIndex + 1 >= currentPath.childCount)
            {
                Debug.Log($"{name} reached end of path {currentPathName}!");
                yield break;
            }

            currentIndex++;
            Vector3 nextPos = currentPath.GetChild(currentIndex).position;

            while (Vector3.Distance(transform.position, nextPos) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, nextPos, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }
}
