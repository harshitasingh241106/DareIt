using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiceManager : MonoBehaviour
{
    public static DiceManager Instance;
    public bool CanRoll { get; set; } = true;

    [Header("🎲 Dice UI")]
    public Button[] diceButtons = new Button[3];
    public TMP_Text[] diceTexts = new TMP_Text[3];

    [Header("⚙️ Roll Settings")]
    public float rollDuration = 1.0f;
    public float rollSpeed = 0.1f;

    private int[] diceValues = new int[3];
    private bool[] diceUsed = new bool[3];
    public bool IsRolling { get; private set; }

    public int selectedNumber { get; set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        for (int i = 0; i < diceButtons.Length; i++)
        {
            int idx = i;
            diceButtons[i].onClick.AddListener(() => HandleDiceClick(idx));
            diceButtons[i].gameObject.SetActive(false);
        }
    }

    public void StartRoll()
    {
        if (!CanRoll || IsRolling) return;
        StartCoroutine(RollAnimation());
    }


    private IEnumerator RollAnimation()
    {
        IsRolling = true;

        for (int i = 0; i < 3; i++)
        {
            diceTexts[i].text = "";
            diceButtons[i].interactable = false;
            diceButtons[i].gameObject.SetActive(true);
            diceUsed[i] = false;
        }

        float elapsed = 0f;
        while (elapsed < rollDuration)
        {
            for (int i = 0; i < 3; i++)
                diceTexts[i].text = Random.Range(1, 7).ToString();
            elapsed += rollSpeed;
            yield return new WaitForSeconds(rollSpeed);
        }

        for (int i = 0; i < 3; i++)
        {
            diceValues[i] = Random.Range(1, 7);
            diceTexts[i].text = diceValues[i].ToString();
            diceButtons[i].interactable = true;
            diceButtons[i].GetComponent<Image>().color = Color.white;
        }

        IsRolling = false;
        CanRoll = false; // ❌ prevent re-roll until all dice used
    }
    // Returns the last rolled dice values for reuse (enemy reading UI dice values)
    public int[] GetRolledValues()
    {
        int[] copy = new int[3];
        for (int i = 0; i < 3; i++)
        {
            int parsed;
            if (int.TryParse(diceTexts[i].text, out parsed))
                copy[i] = parsed;
            else
                copy[i] = Random.Range(1, 7);
        }
        return copy;
    }

    // Highlights currently used dice visually (for enemy turn)
    public void HighlightDieForValue(int value)
    {
        for (int i = 0; i < diceButtons.Length; i++)
        {
            if (diceTexts[i].text == value.ToString())
            {
                diceButtons[i].GetComponent<Image>().color = Color.yellow;
                return;
            }
        }
    }

    // 🌀 Find nearest teleport tile on the current path



    private void HandleDiceClick(int index)
    {
        if (diceUsed[index]) return;
        if (GameManager.Instance.IsWaitingForPieceSelection()) return;

        selectedNumber = diceValues[index];
        diceUsed[index] = true;
        diceButtons[index].interactable = false;
        diceButtons[index].GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f);

        DisableAllDiceExcept(index);

        GameManager.Instance.OnDiceSelected(index, selectedNumber);
        Debug.Log($"🎯 Dice {index + 1} selected: {selectedNumber}");
    }

    public void SetDieUsed(int index)
    {
        if (index < 0 || index >= diceButtons.Length) return;
        diceButtons[index].interactable = false;
        diceButtons[index].GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f);
    }

    public void ResetDice()
    {
        for (int i = 0; i < 3; i++)
        {
            diceButtons[i].gameObject.SetActive(false);
            diceTexts[i].text = "";
            diceUsed[i] = false;
        }
        selectedNumber = 0;
    }

    public bool AllDiceUsed()
    {
        foreach (bool used in diceUsed)
            if (!used) return false;
        return true;
    }


    public void DisableAllDiceExcept(int usedIndex)
    {
        for (int i = 0; i < diceButtons.Length; i++)
        {
            if (i != usedIndex)
            {
                diceButtons[i].interactable = false;
                diceButtons[i].GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f);
            }
        }
    }

    public void EnableUnusedDice()
    {
        for (int i = 0; i < diceButtons.Length; i++)
        {
            if (!diceUsed[i])
            {
                diceButtons[i].interactable = true;
                diceButtons[i].GetComponent<Image>().color = Color.white;
            }
        }
    }
    public void ForceRollForEnemy()
    {
        for (int i = 0; i < diceButtons.Length; i++)
        {
            diceButtons[i].gameObject.SetActive(true);
            diceTexts[i].text = "";
            diceButtons[i].interactable = false;
            diceUsed[i] = false;
            diceButtons[i].GetComponent<Image>().color = Color.white;
        }

        CanRoll = true;
        StartCoroutine(RollAnimation());
    }


}
