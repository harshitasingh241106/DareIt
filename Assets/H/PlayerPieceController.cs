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
    public bool hasMovedThisTurn = false;

    void Start()
    {
        // keep as before: button click to select piece
        GetComponent<Button>().onClick.AddListener(OnPieceClicked);
    }

    void OnPieceClicked()
    {
        Debug.Log($"Clicked on: {name}  (instanceID: {GetInstanceID()})");
        // BLOCK selection while a box event UI is active
        if (GameManager.Instance != null && GameManager.Instance.IsBoxEventActive) return;

        if (DiceManager.Instance.selectedNumber <= 0)
        {
            Debug.Log("⚠️ Select dice first!");
            return;
        }

        if (hasMovedThisTurn)
        {
            Debug.Log($"⚠️ {name} has already moved this turn!");
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

    public IEnumerator MoveAlongPath(int steps, bool moveBackward)
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

        // TELEPORTATION: left exactly as you had it
        if (currentTile.CompareTag("Teleportation_tile"))
        {
            Debug.Log("⚡ Teleportation tile reached!");
            GameManager.Instance.OnTeleportationTileReached(this, currentTile);
            yield break; // teleportation handler will continue flow (it calls MovePiece or OnPieceMoved inside)
        }

        // IMPORTANT: keep original flow — call OnPieceMoved which will now also check for box tiles.
        GameManager.Instance.OnPieceMoved(this);
    }

    // ensure a destroy function exists for bombs
    public void DestroyPiece()
    {
        isOnBoard = false;
        gameObject.SetActive(false);
        // optionally notify GameManager to check loss conditions
        // GameManager.Instance.CheckPlayerLoss(); // implement if needed
    }
}
