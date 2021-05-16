using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPausaManager : MonoBehaviour
{
    public void ResumeButton()
    {
        SceneManager.LoadScene("water_islands");
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