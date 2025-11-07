using System.Collections;
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
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetMouseButtonDown(1))
            {
                Debug.Log("⬅️ Moving backward.");
                selectedPiece.MovePiece(selectedDiceValue, moveBackward: true);
                EndTurn();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetMouseButtonDown(0))
            {
                Debug.Log("➡️ Moving forward.");
                selectedPiece.MovePiece(selectedDiceValue, moveBackward: false);
                EndTurn();
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

    public void OnPieceMoved(PlayerPieceController piece)
    {
        Debug.Log($"✅ {piece.name} finished moving.");
    }

    private void EndTurn()
    {
        ResetTurn();
        Debug.Log("🔄 Turn ended.");
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
