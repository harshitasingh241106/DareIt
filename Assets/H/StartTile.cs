using UnityEngine;
using UnityEngine.UI;

public class StartTileManager : MonoBehaviour
{
    public static StartTileManager Instance;

    [System.Serializable]
    public class StartTileLink
    {
        public Button uiButton;
        public Transform linkedTile;     // invisible tile in scene
        public Transform linkedPath;     // path parent that contains this tile
    }

    public StartTileLink[] startTiles;

    void Awake()
    {
        Instance = this;

        foreach (var st in startTiles)
        {
            st.uiButton.onClick.AddListener(() => OnStartTileClicked(st));
            st.uiButton.interactable = false;
        }
    }

    private void OnStartTileClicked(StartTileLink st)
    {
        GameManager.Instance.OnStartTileChosen(st.linkedTile, st.linkedPath);
    }

    public void EnableStartTileButtons(bool enable)
    {
        foreach (var st in startTiles)
        {
            st.uiButton.interactable = enable;
        }
    }

    // -------------------- NEW: Random start tile --------------------
    public Transform GetRandomStartTile()
    {
        if (startTiles == null || startTiles.Length == 0) return null;

        int idx = Random.Range(0, startTiles.Length);
        return startTiles[idx].linkedTile;
    }
}
