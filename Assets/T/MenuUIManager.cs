using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mainMenuPanel;   // 👈 your main menu buttons parent
    public GameObject rulesPopup;      // 👈 your instructions popup

    public void ShowRules()
    {
        mainMenuPanel.SetActive(false);  // 👈 hide menu
        rulesPopup.SetActive(true);      // 👈 show popup
    }

    public void BackToMenu()
    {
        rulesPopup.SetActive(false);     // 👈 hide popup
        mainMenuPanel.SetActive(true);   // 👈 show menu
    }

    public void StartGame()
    {
        SceneManager.LoadScene("MainGameScene");
    }
}
