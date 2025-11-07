using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerPieceController : MonoBehaviour
{
    public static PlayerPieceController selectedPiece;

    public bool isOnBoard = false;
    public Transform currentPath;
    public int currentIndex = 0;
    public float moveSpeed = 3f;
    public string currentPathName;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnPieceClicked);
    }

    void OnPieceClicked()
    {
        if (DiceManager.Instance.selectedNumber <= 0)
        {
            Debug.Log("⚠️ Select dice first!");
            return;
        }

        selectedPiece = this;
        Debug.Log($"✅ Selected piece: {name}");
    }

    public void PlaceOnStartTile(Transform startTile, Transform pathHolder)
    {
        transform.position = startTile.position;
        currentPath = pathHolder;
        currentPathName = pathHolder.name;
        isOnBoard = true;
        currentIndex = 0;
    }

    public void MovePiece(int steps, bool moveBackward = false)
    {
        if (!isOnBoard || currentPath == null) return;
        StartCoroutine(MoveAlongPath(steps, moveBackward));
    }

    private IEnumerator MoveAlongPath(int steps, bool moveBackward)
    {

        for (int i = 0; i < steps; i++)
        {
            if (!moveBackward)
            {
                if (currentIndex + 1 >= currentPath.childCount) yield break;
                currentIndex++;
            }
            else
            {
                if (currentIndex - 1 < 0) yield break;
                currentIndex--;
            }

            Vector3 nextPos = currentPath.GetChild(currentIndex).position;
            while (Vector3.Distance(transform.position, nextPos) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, nextPos, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }

        Transform currentTile = currentPath.GetChild(currentIndex);

        if (currentTile.CompareTag("Teleportation_tile"))
        {
            Debug.Log("⚡ Teleportation tile reached!");
            GameManager.Instance.OnTeleportationTileReached(this, currentTile);
        }

        GameManager.Instance.OnPieceMoved(this);

    }

}
