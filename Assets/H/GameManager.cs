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

    [Header("Enemy")]
    public EnemyPieceController[] enemyPieces;
    public Transform enemyParent;
    public GameObject enemyPiecePrefab;
    public bool isEnemyTurn = false;

    private PlayerPieceController selectedPiece;
    private int selectedDiceValue;
    private bool isWaitingForPiece;
    private bool isWaitingForStartTile;
    private bool isWaitingForMoveDirection;
    public GameObject boxChoiceUIPrefab;
    public GameObject boxUsedMarkerPrefab;
    private GameObject activeBoxUI;
    public bool IsBoxEventActive { get; private set; } = false;

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
        if (isEnemyTurn) return;

        // roll dice
        if (Input.GetKeyDown(KeyCode.Space) && !diceManager.IsRolling && diceManager.CanRoll)
            StartCoroutine(StartNewTurn());

        // when player selects piece
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
                Debug.Log("↔ Choose direction: Left=Backward, Right=Forward.");
            }

            PlayerPieceController.selectedPiece = null;
        }

        // when direction chosen
        // when direction chosen
        if (isWaitingForMoveDirection && selectedPiece != null)
        {
            int currentIndex = selectedPiece.currentIndex;
            Transform path = selectedPiece.currentPath;
            int maxIndex = path.childCount - 1;

            bool canMoveForward = currentIndex + selectedDiceValue <= maxIndex;
            bool canMoveBackward = currentIndex - selectedDiceValue >= 0;

            // 🧱 case 1: no move possible at all
            if (!canMoveForward && !canMoveBackward)
            {
                Debug.LogWarning("❌ No valid move possible for this piece!");
                ResetMoveSelection();
                diceManager.EnableUnusedDice();
                return;
            }

            // 🟩 case 2: forward direction chosen
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (canMoveForward)
                {
                    selectedPiece.MovePiece(selectedDiceValue, false);
                    isWaitingForMoveDirection = false;
                }
                else
                {
                    Debug.Log("⚠️ Forward move not allowed — at end tile!");

                    ResetMoveSelection();
                    diceManager.EnableUnusedDice();
                    isWaitingForPiece = true;  // ✅ allow selecting another piece
                }
            }
            // 🟨 case 3: backward direction chosen
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (canMoveBackward)
                {
                    selectedPiece.MovePiece(selectedDiceValue, true);
                    isWaitingForMoveDirection = false;
                }
                else
                {
                    Debug.Log("⚠️ Backward move not allowed — at start tile!");
                    ResetMoveSelection();
                    diceManager.EnableUnusedDice();
                    isWaitingForPiece = true;  // ✅ allow retry
                }
            }
        }
    }

    private IEnumerator StartNewTurn()
    {
        Debug.Log("🎲 Player rolling dice...");
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

        // 🧠 prefer teleport tiles on paths that have at least one player
        List<(Transform tile, Transform pathParent)> candidateTeleports = new List<(Transform, Transform)>();
        foreach (var tp in otherPathTeleports)
        {
            foreach (var p in playerPieces)
            {
                if (p.isOnBoard && p.currentPath == tp.pathParent)
                {
                    candidateTeleports.Add(tp);
                    break;
                }
            }
        }

        // if no targeted teleport found, fallback to random
        var randomTarget = candidateTeleports.Count > 0
            ? candidateTeleports[Random.Range(0, candidateTeleports.Count)]
            : otherPathTeleports[Random.Range(0, otherPathTeleports.Count)];

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
        Debug.Log($"Actually moving: {piece.name}");

        if (piece == null) return;

        // find current tile
        GameObject currentTileGO = null;
        if (piece.currentPath != null && piece.currentIndex >= 0 && piece.currentIndex < piece.currentPath.childCount)
            currentTileGO = piece.currentPath.GetChild(piece.currentIndex).gameObject;

        // 🟫 BOX TILE LOGIC
        if (currentTileGO != null && currentTileGO.CompareTag("Box_tile"))
        {
            BoxTile box = currentTileGO.GetComponent<BoxTile>();

            if (box != null && !box.IsUsed)
            {
                // ✅ Check if prefab/canvas available
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null && boxChoiceUIPrefab != null)
                {
                    GameObject ui = Instantiate(boxChoiceUIPrefab, canvas.transform);
                    var uiComp = ui.GetComponent<BoxChoiceUI>();
                    if (uiComp != null)
                        uiComp.Setup(piece, currentTileGO);
                }
                else
                {
                    Debug.LogError("❌ Missing Canvas or BoxChoiceUIPrefab!");
                }

                return; // wait for UI choice
            }
        }


        piece.hasMovedThisTurn = true;
        ResetMoveSelection();
        Debug.Log($"✅ {piece.name} finished moving.");

        if (!isEnemyTurn)
        {
            if (diceManager.AllDiceUsed())
            {
                Debug.Log("🔄 Player turn ended. Enemy turn starting...");
                StartCoroutine(EnemyTurnCoroutine());
            }
            else
            {
                diceManager.EnableUnusedDice();
            }
        }
    }
    private Transform FindNearestTeleportTile(EnemyPieceController enemy)
    {
        if (enemy.currentPath == null) return null;

        float shortestDist = float.MaxValue;
        Transform nearestTile = null;

        for (int i = 0; i < enemy.currentPath.childCount; i++)
        {
            Transform tile = enemy.currentPath.GetChild(i);
            if (!tile.CompareTag("Teleportation_tile")) continue;

            float dist = Mathf.Abs(i - enemy.currentIndex);
            if (dist < shortestDist)
            {
                shortestDist = dist;
                nearestTile = tile;
            }
        }

        return nearestTile;
    }
    // 🧠 Predict where a player might go next turn (rough estimate)
    private int PredictPlayerNextIndex(PlayerPieceController player)
    {
        // Assume typical dice range (1–6) and average move
        int averageMove = 3;

        int predicted = player.currentIndex + averageMove;
        if (predicted >= player.currentPath.childCount)
            predicted = player.currentPath.childCount - 1;

        return predicted;
    }
    // 🧭 Predictive targeting: find player whose future position will be nearest after teleport
    private PlayerPieceController FindBestFutureTarget(EnemyPieceController enemy, List<(Transform tile, Transform pathParent)> teleports)
    {
        PlayerPieceController bestPlayer = null;
        float bestDistance = float.MaxValue;
        Transform bestTeleport = null;

        foreach (var tp in teleports)
        {
            foreach (var p in playerPieces)
            {
                if (!p.isOnBoard) continue;

                int predictedIndex = PredictPlayerNextIndex(p);
                if (tp.pathParent != p.currentPath) continue;

                float distance = Mathf.Abs(predictedIndex - tp.tile.GetSiblingIndex());
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestPlayer = p;
                    bestTeleport = tp.tile;
                }
            }
        }

        if (bestPlayer != null)
            Debug.Log($"🧠 {enemy.name} predicts {bestPlayer.name} will be near teleport on {bestPlayer.currentPath.name}");

        return bestPlayer;
    }

    // ---------------- BOX HANDLING ----------------

    public void ResolveBoxChoice(PlayerPieceController piece, GameObject tile, bool open)
    {
        if (tile == null)
        {
            Debug.LogError("❌ Box tile missing during ResolveBoxChoice!");
            EndBoxEvent(piece);
            return;
        }

        BoxTile box = tile.GetComponent<BoxTile>();
        if (box == null)
        {
            Debug.LogWarning("⚠️ Tile has no BoxTile script. Treating as Empty.");
            EndBoxEvent(piece);
            return;
        }

        // Mark box as used
        box.MarkUsed();

        if (!open)
        {
            Debug.Log("📦 Player ignored the box.");
            EndBoxEvent(piece);
            return;
        }

        switch (box.type)
        {
            case BoxTile.BoxType.Bomb:
                Debug.Log("💣 Boom! This box was a bomb!");
                piece.DestroyPiece();
                break;

            case BoxTile.BoxType.Reward:
                Debug.Log("🎁 Reward! Player gets an extra dice roll!");
                diceManager.EnableUnusedDice();
                break;

            case BoxTile.BoxType.Destination:
                Debug.Log("🏁 Destination reached! Player wins!");
                PlayerWins(piece);
                break;

            case BoxTile.BoxType.Empty:
                Debug.Log("📦 Empty box... nothing happened.");
                break;
        }

        EndBoxEvent(piece);
    }

    private void EndBoxEvent(PlayerPieceController piece)
    {
        Debug.Log("📦 Box event ended.");
        OnPieceMoved(piece);
    }



    private IEnumerator EnemyTurnCoroutine()
    {
        isEnemyTurn = true;
        Debug.Log("🤖 Enemy turn started...");

        // 🛑 Skip if all players dead
        bool anyAlive = false;
        foreach (var p in playerPieces)
            if (p.isOnBoard) anyAlive = true;

        if (!anyAlive)
        {
            Debug.Log("💀 All players dead — enemy skipping turn.");
            yield break;
        }

        // 🎲 Enemy visible dice roll
        diceManager.ResetDice();
        yield return new WaitForSeconds(0.5f);
        diceManager.ForceRollForEnemy();
        yield return new WaitForSeconds(diceManager.rollDuration + 0.5f);

        int[] diceValues = diceManager.GetRolledValues();
        List<int> unusedDice = new List<int>(diceValues);
        Debug.Log($"🎲 Enemy rolled: {string.Join(", ", diceValues)}");

        // Ensure enemyPieces array filled
        if (enemyParent != null && (enemyPieces == null || enemyPieces.Length == 0))
            enemyPieces = enemyParent.GetComponentsInChildren<EnemyPieceController>();

        // Select up to 3 random active enemies
        List<EnemyPieceController> enemiesToMove = new List<EnemyPieceController>();
        List<EnemyPieceController> pool = new List<EnemyPieceController>(enemyPieces);

        for (int i = 0; i < 3 && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
            enemiesToMove.Add(pool[idx]);
            pool.RemoveAt(idx);
        }

        // 🧩 Move selected enemies strategically
        foreach (EnemyPieceController e in enemiesToMove)
        {
            if (unusedDice.Count == 0) break; // all dice used

            // pick random available dice first
            int diceValue = unusedDice[Random.Range(0, unusedDice.Count)];
            unusedDice.Remove(diceValue);

            // Spawn if not placed
            if (e.currentPath == null)
            {
                Transform randomStart = StartTileManager.Instance.GetRandomStartTile();
                e.SpawnAtTile(randomStart.parent, randomStart.GetSiblingIndex());
                Debug.Log($"🟢 {e.name} spawned at start.");
                yield return new WaitForSeconds(0.5f);
            }

            // Find nearest player on same path
            PlayerPieceController target = FindNearestPlayer(e);
            bool moveBackward = false;

            if (target != null)
            {
                int distance = target.currentIndex - e.currentIndex;
                moveBackward = distance < 0;

                Debug.Log($"🎯 {e.name} chasing {target.name} {(moveBackward ? "backward" : "forward")} (dist={Mathf.Abs(distance)})");

                // check if any unused dice matches exact kill
                foreach (int d in new List<int>(unusedDice))
                {
                    int predicted = e.currentIndex + (moveBackward ? -d : d);
                    if (predicted == target.currentIndex)
                    {
                        diceValue = d;
                        unusedDice.Remove(d);
                        Debug.Log($"💀 {e.name} found perfect kill with dice {d}");
                        break;
                    }
                }
            }
            else
            {
                // 🧠 No player on this path — plan teleport toward likely player path
                var allTeleports = pathManager.GetAllTeleportationTiles();

                // Predict which teleport leads to path where player is headed
                PlayerPieceController predictedTarget = FindBestFutureTarget(e, allTeleports);

                if (predictedTarget != null)
                {
                    // find teleport on current path that can connect closer to target's path
                    Transform teleportHere = FindNearestTeleportTile(e);
                    if (teleportHere != null)
                    {
                        int tpIndex = teleportHere.GetSiblingIndex();
                        moveBackward = tpIndex < e.currentIndex;
                        Debug.Log($"🔮 {e.name} predicts {predictedTarget.name} will reach {predictedTarget.currentPath.name}, moving to teleport {tpIndex}");
                    }
                    else
                    {
                        moveBackward = Random.value > 0.5f;
                        Debug.Log($"🤖 {e.name} could not find teleport, moving randomly.");
                    }
                }
                else
                {
                    // fallback: no predictive path found
                    Transform teleportHere = FindNearestTeleportTile(e);
                    if (teleportHere != null)
                    {
                        int tpIndex = teleportHere.GetSiblingIndex();
                        moveBackward = tpIndex < e.currentIndex;
                        Debug.Log($"🌀 {e.name} heading toward teleport tile (fallback) {tpIndex}");
                    }
                    else
                    {
                        moveBackward = Random.value > 0.5f;
                        Debug.Log($"🤖 {e.name} moving randomly (no teleports).");
                    }
                }
            }



            // Highlight dice visually
            diceManager.HighlightDieForValue(diceValue);
            yield return new WaitForSeconds(0.7f);

            // Actually move
            yield return StartCoroutine(e.MoveAlongPath(diceValue, moveBackward));

            OnEnemyPieceLanded(e);

            yield return new WaitForSeconds(0.8f);
        }

        // ✅ Turn end
        Debug.Log("✅ Enemy turn ended. Back to Player turn...");
        yield return new WaitForSeconds(0.5f);

        diceManager.ResetDice();
        diceManager.CanRoll = true;
        isEnemyTurn = false;

        foreach (var p in playerPieces)
            p.hasMovedThisTurn = false;
    }


    // 🧩 When an enemy lands on a player tile
    public void OnEnemyPieceLanded(EnemyPieceController enemy)
    {
        // Find if any player piece is on the same tile
        PlayerPieceController target = FindPlayerPieceOnTile(enemy.currentPath, enemy.currentIndex);

        if (target != null && target.isOnBoard)
        {
            Debug.Log($"💀 Enemy {enemy.name} captured {target.name}!");

            // Destroy the player piece
            target.DestroyPiece();

            // Spawn a new enemy piece at that position
            GameObject newEnemyObj = Instantiate(enemyPiecePrefab, target.transform.position, Quaternion.identity, enemyParent);
            EnemyPieceController newEnemy = newEnemyObj.GetComponent<EnemyPieceController>();

            // Initialize new enemy's position & path
            newEnemy.SpawnAtTile(enemy.currentPath, enemy.currentIndex);

            // Add to enemy list
            AddEnemyPiece(newEnemy);

            // Check if all player pieces are dead
            CheckForGameOver();
        }
    }
    private PlayerPieceController FindNearestPlayer(EnemyPieceController enemy)
    {
        PlayerPieceController nearest = null;
        float shortestDistance = float.MaxValue;

        foreach (var p in playerPieces)
        {
            if (!p.isOnBoard) continue;
            if (p.currentPath != enemy.currentPath) continue;

            float dist = Mathf.Abs(p.currentIndex - enemy.currentIndex);
            if (dist < shortestDistance)
            {
                shortestDistance = dist;
                nearest = p;
            }
        }
        return nearest;
    }

    // 🧩 Helper to find any player piece at a given tile
    private PlayerPieceController FindPlayerPieceOnTile(Transform path, int index)
    {
        foreach (var p in playerPieces)
        {
            if (p.isOnBoard && p.currentPath == path && p.currentIndex == index)
                return p;
        }
        return null;
    }

    // 🧩 Helper to add new enemy piece dynamically
    private void AddEnemyPiece(EnemyPieceController newEnemy)
    {
        List<EnemyPieceController> temp = new List<EnemyPieceController>(enemyPieces);
        temp.Add(newEnemy);
        enemyPieces = temp.ToArray();
    }

    // 🧩 Game Over check
    private void CheckForGameOver()
    {
        int alivePlayers = 0;
        foreach (var p in playerPieces)
        {
            if (p.isOnBoard)
                alivePlayers++;
        }

        if (alivePlayers == 0)
        {
            Debug.Log("💀 All player pieces destroyed! GAME OVER!");
            GameOver();
        }
    }

    private void GameOver()
    {
        diceManager.CanRoll = false;
        isEnemyTurn = false;

        // TODO: add your game over UI / scene transition here
        Debug.Log("🕹️ GAME OVER: The player is trapped in the Chakravyuh!");
    }

    public void PlayerWins(PlayerPieceController piece)
    {
        Debug.Log($"🏁 {piece.name} escaped the Chakravyuha!");

        // yahan tu win UI dikhana, buttons disable karna, etc. likh sakta hai
        // Example: 
        // UIManager.Instance.ShowWinScreen();

        // abhi ke liye bas turn end karte hain
        diceManager.CanRoll = false;
        isEnemyTurn = false;
    }


    private void ResetMoveSelection()
    {
        selectedPiece = null;
        isWaitingForPiece = false;
        isWaitingForMoveDirection = false;
        isWaitingForStartTile = false;
        DiceManager.Instance.selectedNumber = 0;  // ✅ ensures clean state
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
        isEnemyTurn = false;
    }

    public bool IsWaitingForPieceSelection()
    {
        return isWaitingForPiece || isWaitingForMoveDirection || isWaitingForStartTile;
    }
}
