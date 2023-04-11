using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject menuObject;
    [SerializeField] private PlayerControls player;

    private void Update()
    {
        if (!GameManager.Instance.IsLoading && Input.GetKeyDown(KeyCode.Escape))
        {
            if (menuObject.activeInHierarchy)
            {
                HideMenu();
            }
            else
            {
                ShowMenu();
            }
        }
    }

    public void ShowMenu()
    {
        menuObject.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        player.SetMovementActive(false);
        Time.timeScale = 0f;
    }

    public void HideMenu()
    {
        menuObject.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        player.SetMovementActive(true);
        Time.timeScale = 1.0f;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
