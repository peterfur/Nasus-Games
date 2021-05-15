using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPausaManager : MonoBehaviour
{
    public void ResumeButton()
    {
        SceneManager.LoadScene("SampleScene1");
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