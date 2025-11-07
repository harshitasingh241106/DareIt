using UnityEngine;
using UnityEngine.UI;
using TMPro; // Add this for TextMeshPro
using System.Collections.Generic;

public class NumberRoller : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button button1;
    public Button button2;
    public Button button3;

    private List<int> rolledNumbers = new List<int>();
    private bool[] usedNumbers = new bool[3];

    void Start()
    {
        // Assign button click events
        button1.onClick.AddListener(() => SelectNumber(0));
        button2.onClick.AddListener(() => SelectNumber(1));
        button3.onClick.AddListener(() => SelectNumber(2));

        ResetButtons();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RollNumbers();
        }

        if (rolledNumbers.Count > 0)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectNumber(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SelectNumber(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SelectNumber(2);
        }
    }

    void RollNumbers()
    {
        rolledNumbers.Clear();
        usedNumbers = new bool[3];

        for (int i = 0; i < 3; i++)
        {
            rolledNumbers.Add(Random.Range(1, 7)); // 1-6 dice
        }

        // Update button texts using TMP
        button1.GetComponentInChildren<TMP_Text>().text = rolledNumbers[0].ToString();
        button2.GetComponentInChildren<TMP_Text>().text = rolledNumbers[1].ToString();
        button3.GetComponentInChildren<TMP_Text>().text = rolledNumbers[2].ToString();

        ResetButtons();

        Debug.Log("Rolled Numbers: " + string.Join(", ", rolledNumbers));
    }

    public void SelectNumber(int index)
    {
        if (index < 0 || index >= rolledNumbers.Count) return;
        if (usedNumbers[index])
        {
            Debug.Log("Number already used this turn!");
            return;
        }

        usedNumbers[index] = true;
        int selectedNumber = rolledNumbers[index];
        Debug.Log("Selected Number: " + selectedNumber);

        // TODO: Move piece here

        // Disable the button
        switch (index)
        {
            case 0: button1.interactable = false; break;
            case 1: button2.interactable = false; break;
            case 2: button3.interactable = false; break;
        }

        if (AllNumbersUsed())
        {
            Debug.Log("All numbers used. Turn over!");
            // TODO: End turn logic
        }
    }

    private bool AllNumbersUsed()
    {
        foreach (bool used in usedNumbers)
            if (!used) return false;
        return true;
    }

    private void ResetButtons()
    {
        button1.interactable = true;
        button2.interactable = true;
        button3.interactable = true;
    }
}
