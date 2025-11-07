using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiceManager : MonoBehaviour
{
    public static DiceManager Instance;

    [Header("🎲 Dice UI")]
    public Button[] diceButtons = new Button[3];
    public TMP_Text[] diceTexts = new TMP_Text[3];

    [Header("⚙️ Roll Settings")]
    public float rollDuration = 1.0f;
    public float rollSpeed = 0.1f;

    private int[] diceValues = new int[3];
    private bool[] diceUsed = new bool[3];
    public bool IsRolling { get; private set; }

    public int selectedNumber { get; private set; }   // ✅ for backward compatibility

    public event System.Action<int, int> OnDiceSelected;

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
        if (!IsRolling)
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
    }

    private void HandleDiceClick(int index)
    {
        if (diceUsed[index]) return;

        diceUsed[index] = true;
        diceButtons[index].interactable = false;
        diceButtons[index].GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f);

        selectedNumber = diceValues[index]; // ✅ added for old references
        Debug.Log($"🎯 Dice {index + 1} selected: {selectedNumber}");

        OnDiceSelected?.Invoke(index, selectedNumber);
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
    }

    public int GetDiceValue(int index)
    {
        if (index < 0 || index >= diceValues.Length) return 0;
        return diceValues[index];
    }

    public bool AllDiceUsed()
    {
        foreach (bool used in diceUsed)
            if (!used) return false;
        return true;
    }
}
