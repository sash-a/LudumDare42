using UnityEngine;
using UnityEngine.SceneManagement;
using XboxCtrlrInput;

public class SceneSwitcher : MonoBehaviour
{

    public AudioSource startSound;

    public void playGame()
    {
        if (XCI.GetNumPluggedCtrlrs() > 0)
        {
            GameInfo.numPlayers = XCI.GetNumPluggedCtrlrs();
            GameInfo.usingControllers = true;
        }
        else
        {
            GameInfo.numPlayers = 1;
            GameInfo.usingControllers = false;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void quitGame()
    {
        Application.Quit();
    }

    private void Update()
    {
        if (Input.GetButtonUp("Submit") || Input.GetButtonUp("SubmitStart"))
        {
            GameInfo.numPlayers = XCI.GetNumPluggedCtrlrs();
            GameInfo.usingControllers = true;

            //Sound
            startSound.Play();

            playGame();
        }

        if (Input.GetButton("Cancel"))
        {
            quitGame();
        }
    }

    public void creditScene()
    {
        SceneManager.LoadScene(2);
    }

    public void controlsScene()
    {
        SceneManager.LoadScene(3);
    }

    public void mainMenuScene()
    {
        SceneManager.LoadScene(0);
    }
}