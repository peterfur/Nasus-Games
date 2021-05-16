using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMuerteManager : MonoBehaviour
{
    public void RestartButton()
    {
        SceneManager.LoadScene("water_islands");//Cambiar Esto, debería de cargar la escena en la que se ha muerto.
    }

    public void ExitButton()
    {
        Debug.Log("Cerramos el juego");
        Application.Quit();
    }
}
