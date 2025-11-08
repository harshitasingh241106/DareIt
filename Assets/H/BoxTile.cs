using UnityEngine;

public class BoxTile : MonoBehaviour
{
    public enum BoxType
    {
        Reward,
        Bomb,
        Destination,
        Empty
    }

    [Header("🎁 Box Tile Settings")]
    public BoxType type = BoxType.Empty;

    [Tooltip("If true, this box triggers only once per game.")]
    public bool oneTimeUse = true;

    private bool used = false;
    public bool IsUsed => used;

    public void MarkUsed()
    {
        if (oneTimeUse)
            used = true;
    }
}
