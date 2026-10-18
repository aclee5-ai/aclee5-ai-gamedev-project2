using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject player;
    public string menuSceneName = "MainMenu";
    public KeyCode pauseKey = KeyCode.Escape;

    bool paused;

    void Start() { SetPaused(false); }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey)) SetPaused(!paused);
    }

    public void SetPaused(bool value)
    {
        paused = value;
        if (pausePanel != null) pausePanel.SetActive(value);
        Time.timeScale = value ? 0f : 1f;

        if (player != null)
            foreach (MonoBehaviour b in player.GetComponents<MonoBehaviour>())
                b.enabled = !value;

        Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = value;
    }

    public void Resume() { SetPaused(false); }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}