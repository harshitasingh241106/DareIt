using UnityEngine;
using System.Collections;

[System.Serializable]
public class PlayerPiece
{
    public string name;
    public Transform pieceTransform;
    public int pathIndex;
    public int currentTileIndex;
}

public class PlayerPieceController : MonoBehaviour
{
    public PlayerPiece[] pieces;
    public PathManager pathManager;
    public DiceManager diceManager;

    private PlayerPiece selectedPiece;
    private int stepsToMove;
    private bool isMoving = false;
    private int moveDirection = 1;

    void OnEnable()
    {
        diceManager.OnNumberSelected += SetSteps;
    }

    void OnDisable()
    {
        diceManager.OnNumberSelected -= SetSteps;
    }

    void SetSteps(int number)
    {
        stepsToMove = number;
        Debug.Log("Number selected: " + stepsToMove);
        // TODO: Let player select piece (for now assign manually or via UI)
    }

    void Update()
    {
        if (selectedPiece != null && !isMoving && stepsToMove > 0)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                moveDirection = 1;
                StartCoroutine(MovePiece());
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                moveDirection = -1;
                StartCoroutine(MovePiece());
            }
        }
    }

    IEnumerator MovePiece()
    {
        isMoving = true;
        Path path = pathManager.paths[selectedPiece.pathIndex];

        for (int i = 0; i < stepsToMove; i++)
        {
            selectedPiece.currentTileIndex += moveDirection;
            selectedPiece.currentTileIndex = Mathf.Clamp(selectedPiece.currentTileIndex, 0, path.tiles.Length - 1);

            Vector3 targetPos = path.tiles[selectedPiece.currentTileIndex].position;

            while (Vector3.Distance(selectedPiece.pieceTransform.position, targetPos) > 0.01f)
            {
                selectedPiece.pieceTransform.position = Vector3.MoveTowards(selectedPiece.pieceTransform.position, targetPos, Time.deltaTime * 5f);
                yield return null;
            }

            string tileTag = path.tiles[selectedPiece.currentTileIndex].tag;
            if (tileTag == "Teleport")
            {
                // TODO: teleport logic
            }
            else if (tileTag == "End")
            {
                // TODO: handle end of path
            }
        }

        isMoving = false;
        selectedPiece = null;
        stepsToMove = 0;
    }
}
