using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingScript : MonoBehaviour
{
    public void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void ExitGame()
    {
        Application.Quit();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
