using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause_Menu : MonoBehaviour
{
    public static bool GamePaused = false;
    public GameObject pauseUI;

    public AudioSource menuSelect;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GamePaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        if (Input.GetButtonUp("SubmitStart"))
        {
            if (GamePaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        if (Input.GetButtonUp("Submit") && GamePaused)
        {
            loadMenu();
        }

        if (Input.GetButton("Cancel") && GamePaused)
        {
            QuitGame();
        }
    }

    public void Resume()
    {
        menuSelect.Play();

        pauseUI.SetActive(false); // close pause menu
        Time.timeScale = 1; // resume time
        GamePaused = false;
    }

    void Pause()
    {
        menuSelect.Play();

        pauseUI.SetActive(true); // bring up pause menu 
        Time.timeScale = 0; // freeze time (can be used for slowmo ;) )
        GamePaused = true;
    }

    public void loadMenu()
    {
        menuSelect.Play();

        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        menuSelect.Play();

        Debug.Log("Quitting");
        Application.Quit(); //will quit properly with built version
    }
}