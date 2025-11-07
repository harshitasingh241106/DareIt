using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DiceManager : MonoBehaviour
{
    public Button button1, button2, button3;
    public List<int> rolledNumbers = new List<int>();
    private bool[] usedNumbers = new bool[3];
    public int selectedNumber { get; private set; }

    public delegate void NumberSelected(int number);
    public event NumberSelected OnNumberSelected;

    void Start()
    {
        button1.onClick.AddListener(() => SelectNumber(0));
        button2.onClick.AddListener(() => SelectNumber(1));
        button3.onClick.AddListener(() => SelectNumber(2));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            RollNumbers();

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
            rolledNumbers.Add(Random.Range(1, 7));

        button1.GetComponentInChildren<TMP_Text>().text = rolledNumbers[0].ToString();
        button2.GetComponentInChildren<TMP_Text>().text = rolledNumbers[1].ToString();
        button3.GetComponentInChildren<TMP_Text>().text = rolledNumbers[2].ToString();

        button1.interactable = true;
        button2.interactable = true;
        button3.interactable = true;

        selectedNumber = 0;
    }

    void SelectNumber(int index)
    {
        if (usedNumbers[index]) return;
        usedNumbers[index] = true;
        selectedNumber = rolledNumbers[index];
        OnNumberSelected?.Invoke(selectedNumber);

        switch (index)
        {
            case 0: button1.interactable = false; break;
            case 1: button2.interactable = false; break;
            case 2: button3.interactable = false; break;
        }
    }

    public bool AllNumbersUsed()
    {
        foreach (var used in usedNumbers)
            if (!used) return false;
        return true;
    }
}
