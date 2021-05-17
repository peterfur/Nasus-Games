using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPausaManager : MonoBehaviour
{

    private int sceneToContinue;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeButton()
    {

        Time.timeScale = 1;
        SceneManager.UnloadSceneAsync("MenuPausa");
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;


    }

    public void ExitButton()
    {
        Debug.Log("Cerramos el juego");
        Application.Quit();
    }

    public void MainMenuButton()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }

}