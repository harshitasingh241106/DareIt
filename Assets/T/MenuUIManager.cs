using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mainMenuPanel;   // 👈 your main menu buttons parent
    public GameObject rulesPopup;      // 👈 your instructions popup

    // 🧩 Show Rules Popup
    public void ShowRules()
    {
        mainMenuPanel.SetActive(false);
        rulesPopup.SetActive(true);
    }

    // 🧩 Back from Rules to Menu
    public void BackToMenu()
    {
        rulesPopup.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // 🧩 Start the Main Game Scene
    public void StartGame()
    {
        // Load the main game scene by name or index
        SceneManager.LoadScene("SampleScene");
        Debug.Log("🎮 Loading Main Game Scene...");
    }

    // 🧩 Exit Game
    public void ExitGame()
    {
        Debug.Log("🚪 Exiting game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;  // stops play mode in editor
#else
        Application.Quit();  // quits build app
#endif
    }
}
