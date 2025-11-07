using System.Collections;
using UnityEngine;
using System.Collections.Generic;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    public DiceManager diceManager;
    public PathManager pathManager;
    public PlayerPieceController[] playerPieces;

    private PlayerPieceController selectedPiece;
    private int selectedDiceValue;
    private bool isWaitingForPiece;
    private bool isWaitingForStartTile;
    private bool isWaitingForMoveDirection;
    private bool isPlayerTurn = true;
    public EnemyPieceController[] enemyPieces;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        diceManager.OnDiceSelected += OnDiceSelected;
        ResetTurn();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !diceManager.IsRolling)
        {
            StartCoroutine(StartNewTurn());
        }

        if (PlayerPieceController.selectedPiece != null && isWaitingForPiece)
        {
            selectedPiece = PlayerPieceController.selectedPiece;
            isWaitingForPiece = false;

            if (!selectedPiece.isOnBoard)
            {
                Debug.Log("🟩 Piece selected. Waiting for start tile click...");
                isWaitingForStartTile = true;
                StartTileManager.Instance.EnableStartTileButtons(true);
            }
            else
            {
                // ✅ Wait for direction input instead of ending turn immediately
                isWaitingForMoveDirection = true;
                Debug.Log("↔️ Choose direction: Left=Backward, Right=Forward.");
            }

            PlayerPieceController.selectedPiece = null;
        }

        // ✅ Handle direction choice when needed
        if (isWaitingForMoveDirection && selectedPiece != null)
        {
            if (isWaitingForMoveDirection && selectedPiece != null)
            {
                int currentIndex = selectedPiece.currentIndex;
                Transform path = selectedPiece.currentPath;
                int maxIndex = path.childCount - 1;

                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetMouseButtonDown(1))
                {
                    Debug.Log("⬅️ Moving backward.");

                    // ✅ BACKWARD VALIDATION
                    if (currentIndex - selectedDiceValue < 0)
                    {
                        Debug.LogWarning("❌ Can't move backward beyond the start tile!");
                        EndTurn();
                        return;
                    }

                    selectedPiece.MovePiece(selectedDiceValue, moveBackward: true);
                    EndTurn();
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetMouseButtonDown(0))
                {
                    Debug.Log("➡️ Moving forward.");

                    // ✅ FORWARD VALIDATION
                    if (currentIndex + selectedDiceValue > maxIndex)
                    {
                        Debug.LogWarning("❌ Can't move beyond the end tile!");
                        EndTurn();
                        return;
                    }

                    selectedPiece.MovePiece(selectedDiceValue, moveBackward: false);
                    EndTurn();
                }
            }

        }
    }

    private IEnumerator StartNewTurn()
    {
        Debug.Log("🎲 Rolling dice...");
        diceManager.ResetDice();
        diceManager.StartRoll();
        yield return new WaitForSeconds(diceManager.rollDuration + 0.5f);
        Debug.Log("✅ Choose one dice number.");
    }

    private void OnDiceSelected(int index, int value)
    {
        selectedDiceValue = value;
        Debug.Log($"🎯 Dice {index + 1} selected: {value}");
        isWaitingForPiece = true;
        diceManager.SetDieUsed(index);
    }

    public void OnStartTileChosen(Transform startTile, Transform pathParent)
    {
        if (selectedPiece == null) return;

        Debug.Log($"🏁 Placing piece {selectedPiece.name} on {pathParent.name}");
        selectedPiece.PlaceOnStartTile(startTile, pathParent);

        isWaitingForStartTile = false;
        StartTileManager.Instance.EnableStartTileButtons(false);

        // ✅ Auto move after placement
        selectedPiece.MovePiece(selectedDiceValue);
        EndTurn();
    }
    public void OnTeleportationTileReached(PlayerPieceController piece, Transform currentTile)
    {
        var allTeleportTiles = pathManager.GetAllTeleportationTiles();

        Debug.Log($"🧩 Found total teleport tiles: {allTeleportTiles.Count}");
        Debug.Log($"🧭 Current path: {piece.currentPath.name}");

        List<(Transform tile, Transform pathParent)> otherPathTeleports = new List<(Transform, Transform)>();

        foreach (var tp in allTeleportTiles)
        {
            Debug.Log($"➡ Tile: {tp.tile.name} | Path: {tp.pathParent.name}");

            if (tp.pathParent != piece.currentPath)
                otherPathTeleports.Add(tp);
        }

        Debug.Log($"🎯 Other-path teleport tiles found: {otherPathTeleports.Count}");

        if (otherPathTeleports.Count == 0)
        {
            Debug.Log("⚠️ No other teleportation tiles found in different paths.");
            return;
        }

        var randomTarget = otherPathTeleports[Random.Range(0, otherPathTeleports.Count)];
        Transform targetTile = randomTarget.tile;
        Transform targetPath = randomTarget.pathParent;

        piece.transform.position = targetTile.position;
        piece.currentPath = targetPath;

        int newIndex = 0;
        for (int i = 0; i < targetPath.childCount; i++)
        {
            if (targetPath.GetChild(i) == targetTile)
            {
                newIndex = i;
                break;
            }
        }
        piece.currentIndex = newIndex;

        Debug.Log($"✨ {piece.name} teleported to {targetTile.name} on path {targetPath.name}");
    }




    public void OnPieceMoved(PlayerPieceController piece)
    {
        Debug.Log($"✅ {piece.name} finished moving.");
    }

    private void EndTurn()
    {
        ResetTurn();

        if (isPlayerTurn)
        {
            isPlayerTurn = false;
            Debug.Log("🤖 Enemy turn begins!");
            StartCoroutine(EnemyTurnRoutine());
        }
        else
        {
            isPlayerTurn = true;
            Debug.Log("🧍 Player turn begins!");
        }
        DiceManager.Instance.CanRoll = true;
    }

    private void ResetTurn()
    {
        selectedDiceValue = 0;
        selectedPiece = null;
        isWaitingForPiece = false;
        isWaitingForStartTile = false;
        isWaitingForMoveDirection = false;
    }
}
