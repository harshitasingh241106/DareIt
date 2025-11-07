using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ResetTurn();
    }

    void Update()
    {
        // roll dice (only when allowed)
        if (Input.GetKeyDown(KeyCode.Space) && !diceManager.IsRolling && diceManager.CanRoll)
            StartCoroutine(StartNewTurn());

        // when player piece clicked
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
                isWaitingForMoveDirection = true;
                Debug.Log("↔️ Choose direction: Left=Backward, Right=Forward.");
            }

            PlayerPieceController.selectedPiece = null;
        }

        // when direction input
        if (isWaitingForMoveDirection && selectedPiece != null)
        {
            int currentIndex = selectedPiece.currentIndex;
            Transform path = selectedPiece.currentPath;
            int maxIndex = path.childCount - 1;

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetMouseButtonDown(1))
            {
                if (currentIndex - selectedDiceValue < 0)
                {
                    Debug.LogWarning("❌ Can't move backward beyond the start tile!");
                    ResetMoveSelection();
                    return;
                }
                selectedPiece.MovePiece(selectedDiceValue, true);
                isWaitingForMoveDirection = false;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetMouseButtonDown(0))
            {
                if (currentIndex + selectedDiceValue > maxIndex)
                {
                    Debug.LogWarning("❌ Can't move beyond the end tile!");
                    ResetMoveSelection();
                    return;
                }
                selectedPiece.MovePiece(selectedDiceValue, false);
                isWaitingForMoveDirection = false;
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

    public void OnDiceSelected(int index, int value)
    {
        selectedDiceValue = value;
        isWaitingForPiece = true;
        diceManager.SetDieUsed(index);
        diceManager.DisableAllDiceExcept(index);
        Debug.Log($"🎯 Dice {index + 1} selected: {value}");
    }

    public void OnStartTileChosen(Transform startTile, Transform pathParent)
    {
        if (selectedPiece == null) return;

        selectedPiece.PlaceOnStartTile(startTile, pathParent);
        isWaitingForStartTile = false;
        StartTileManager.Instance.EnableStartTileButtons(false);

        selectedPiece.MovePiece(selectedDiceValue);
        selectedPiece = null;
    }

    public void OnTeleportationTileReached(PlayerPieceController piece, Transform currentTile)
    {
        var allTeleportTiles = pathManager.GetAllTeleportationTiles();
        List<(Transform tile, Transform pathParent)> otherPathTeleports = new List<(Transform, Transform)>();

        foreach (var tp in allTeleportTiles)
            if (tp.pathParent != piece.currentPath)
                otherPathTeleports.Add(tp);

        if (otherPathTeleports.Count == 0) return;

        var randomTarget = otherPathTeleports[Random.Range(0, otherPathTeleports.Count)];
        Transform targetTile = randomTarget.tile;
        Transform targetPath = randomTarget.pathParent;

        piece.transform.position = targetTile.position;
        piece.currentPath = targetPath;

        int newIndex = 0;
        for (int i = 0; i < targetPath.childCount; i++)
            if (targetPath.GetChild(i) == targetTile) newIndex = i;

        piece.currentIndex = newIndex;
        Debug.Log($"✨ {piece.name} teleported to {targetTile.name} on path {targetPath.name}");

        int maxIndex = targetPath.childCount - 1;
        if (newIndex + 1 <= maxIndex)
            piece.MovePiece(1, false);
        else if (newIndex - 1 >= 0)
            piece.MovePiece(1, true);
        else
            OnPieceMoved(piece);
    }

    public void OnPieceMoved(PlayerPieceController piece)
    {
        piece.hasMovedThisTurn = true;
        ResetMoveSelection();
        Debug.Log($"✅ {piece.name} finished moving.");

        if (diceManager.AllDiceUsed())
        {
            Debug.Log("🔄 All dice used. Turn ended.");
            ResetTurn();
            diceManager.ResetDice();
        }
        else
        {
            // re-enable dice selection for next move
            diceManager.EnableUnusedDice();
        }
    }

    private void ResetMoveSelection()
    {
        selectedPiece = null;
        isWaitingForPiece = false;
        isWaitingForMoveDirection = false;
        isWaitingForStartTile = false;
    }

    private void ResetTurn()
    {
        selectedDiceValue = 0;
        selectedPiece = null;
        isWaitingForPiece = false;
        isWaitingForStartTile = false;
        isWaitingForMoveDirection = false;

        foreach (var piece in playerPieces)
            piece.hasMovedThisTurn = false;

        diceManager.CanRoll = true;
    }

    public bool IsWaitingForPieceSelection()
    {
        return isWaitingForPiece || isWaitingForMoveDirection || isWaitingForStartTile;
    }
}
