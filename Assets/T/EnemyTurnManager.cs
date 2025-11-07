using System.Collections;
using UnityEngine;

public class EnemyTurnManager : MonoBehaviour
{
    public static EnemyTurnManager Instance;

    [Header("References")]
    public DiceManager diceManager;
    public EnemyPieceController[] enemyPieces;
    public PlayerPieceController[] playerPieces;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Called by GameManager at the end of the player's turn
    /// </summary>
    public IEnumerator HandleEnemyTurn(System.Action onTurnComplete)
    {
        Debug.Log("?? Enemy turn begins!");

        yield return new WaitForSeconds(0.5f);

        // ?? Roll dice
        diceManager.ResetDice();
        diceManager.StartRoll();
        yield return new WaitForSeconds(diceManager.rollDuration + 0.5f);

        int diceValue = Random.Range(1, 7);
        Debug.Log($"?? Enemy rolled {diceValue}");

        // ?? Enemy decision logic
        if (enemyPieces.Length >= 4)
        {
            // 1?? & 2?? follow closest player pieces
            enemyPieces[0].FollowClosestPlayer(playerPieces, diceValue);
            yield return new WaitForSeconds(0.5f);

            enemyPieces[1].FollowClosestPlayer(playerPieces, diceValue);
            yield return new WaitForSeconds(0.5f);

            // 3?? go toward reward
            enemyPieces[2].MoveTowardRewardTile(diceValue);
            yield return new WaitForSeconds(0.5f);

            // 4?? go to most unfavourable spot for player
            enemyPieces[3].MoveToBlockOrAttack(playerPieces, diceValue);
        }
        else
        {
            Debug.Log("? Not enough enemy pieces assigned.");
        }

        yield return new WaitForSeconds(2f);
        Debug.Log("?? Enemy turn complete.");
        onTurnComplete?.Invoke();
    }
}
