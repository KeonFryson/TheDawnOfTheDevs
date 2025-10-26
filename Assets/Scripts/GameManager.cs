using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject pauseMenuParent;
    [SerializeField] private GameObject Settings;
    [SerializeField] private GameObject Controls;
    
    //private InventoryMenu inventoryMenuComponent;

    private GameObject currentActiveMenu;
    private void Start()
    {
        HideAllMenus();
       

    }

    void Update()
    {
        // Menu activation is now handled here, not in other scripts
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            // Inventory/Stat menu toggle (E key)
           
            // Pause menu toggle (Escape key)
            if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {

                ToggleMenu(pauseMenuParent, pauseMenu);

            }
        }
    }

  
    public void ToggleMenu(params GameObject[] menusToShow)
    {
        // If any of the menus to show is already active, close all menus
        bool anyActive = false;
        foreach (var menu in menusToShow)
        {
            if (menu != null && menu.activeSelf)
            {
                anyActive = true;
                break;
            }
        }

        if (anyActive)
        {
            HideAllMenus();
            Time.timeScale = 1f;
            
            currentActiveMenu = null;
        }
        else
        {
            HideAllMenus();
            foreach (var menu in menusToShow)
            {
                if (menu != null)
                {
                    menu.SetActive(true);
                    currentActiveMenu = menu;


                }
            }
           
            if (pauseMenu != null && pauseMenu.activeSelf)
            {
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 0f;
            }
        }
    }

    public void HideAllMenus()
    {
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (pauseMenuParent != null) pauseMenuParent.SetActive(false);
        if (Settings != null) Settings.SetActive(false);
        if (Controls != null) Controls.SetActive(false);
        currentActiveMenu = null;
    }

    // For use by MenuButtons
    public void OpenMainMenu()
    {
        HideAllMenus();
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void ResumeGame()
    {
        HideAllMenus();
        Time.timeScale = 1f;
    }
}