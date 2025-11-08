using UnityEngine;
using UnityEngine.UI;

public class BoxChoiceUI : MonoBehaviour
{
    public Button openButton;
    public Button ignoreButton;
    public float yOffset = 80f;

    private PlayerPieceController currentPiece;
    private GameObject currentTile;
    private RectTransform rect;

    void Awake() => rect = GetComponent<RectTransform>();

    public void Setup(PlayerPieceController piece, GameObject tile)
    {
        if (piece == null || tile == null)
        {
            Debug.LogError("❌ BoxChoiceUI.Setup called with null piece or tile");
            Destroy(gameObject);
            return;
        }

        currentPiece = piece;
        currentTile = tile;
        UpdatePosition();

        openButton.onClick.RemoveAllListeners();
        ignoreButton.onClick.RemoveAllListeners();

        openButton.onClick.AddListener(OnClickOpen);
        ignoreButton.onClick.AddListener(OnClickIgnore);
    }

    void Update()
    {
        if (currentPiece != null)
            UpdatePosition();
    }

    void UpdatePosition()
    {
        if (rect == null || currentPiece == null) return;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(currentPiece.transform.position);
        rect.position = screenPos + new Vector3(0, yOffset, 0);
    }

    void OnClickOpen()
    {
        GameManager.Instance.ResolveBoxChoice(currentPiece, currentTile, true);
        Destroy(gameObject);
    }


    void OnClickIgnore()
    {
        GameManager.Instance.ResolveBoxChoice(currentPiece, currentTile, false);
        Destroy(gameObject);
    }
}
