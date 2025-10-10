
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MenuButtons : MonoBehaviour
{
    private GameManager gameManager;

    private void Awake()
    {
       
        gameManager = FindFirstObjectByType<GameManager>();
       
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OpenMainMenu()
    {
        if (gameManager != null)
            gameManager.OpenMainMenu();
        else
            SceneManager.LoadScene(0);
    }

    public void ResumeGame()
    {
        if (gameManager != null)
            gameManager.ResumeGame();
        else
            Time.timeScale = 1f;
    }


}
