using UnityEngine;

public class StartTileManager : MonoBehaviour
{
    [Header("Path Info")]
    public Transform pathHolder;  // Assign the correct path in Inspector
    private DiceManager diceManager;

    private int lastRolledNumber = 0;

    void Start()
    {
        // Find the DiceManager automatically (no singleton needed)
        diceManager = FindObjectOfType<DiceManager>();
        if (diceManager == null)
        {
            Debug.LogError("DiceManager not found in scene!");
            return;
        }

        // Subscribe to the dice number selection event
        diceManager.OnNumberSelected += OnDiceNumberSelected;
    }

    private void OnDiceNumberSelected(int number)
    {
        lastRolledNumber = number;
    }

    public void OnStartTileClicked()
    {
        if (PlayerPieceController.selectedPiece == null)
        {
            Debug.LogWarning("No piece selected!");
            return;
        }

        var piece = PlayerPieceController.selectedPiece;

        // If not on board yet → place on this tile and assign path
        if (!piece.isOnBoard)
        {
            piece.PlaceOnStartTile(transform, pathHolder);
            Debug.Log($"Placed {piece.name} on start tile {name}, Path: {pathHolder.name}");
        }
        else
        {
            // Already on board → move along assigned path
            if (piece.currentPath == pathHolder)
            {
                if (lastRolledNumber > 0)
                {
                    piece.MovePiece(lastRolledNumber);
                    lastRolledNumber = 0; // Reset after use
                    Debug.Log($"{piece.name} moving {lastRolledNumber} steps on path {pathHolder.name}");
                }
                else
                {
                    Debug.Log("Roll and select a number first!");
                }
            }
            else
            {
                Debug.LogWarning($"Cannot move {piece.name} on another path!");
            }
        }
    }

    private void OnDestroy()
    {
        if (diceManager != null)
            diceManager.OnNumberSelected -= OnDiceNumberSelected;
    }

}
