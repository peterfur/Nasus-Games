using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMuerteManager : MonoBehaviour
{
    private int sceneToContinue;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void RestartButton()
    {
        
        sceneToContinue = PlayerPrefs.GetInt("SavedScene");

        if (sceneToContinue != 0)
        {
            Debug.Log("Vale distinto de 0 sceneContinue");
            SceneManager.LoadScene(sceneToContinue);
        }
        else
        {
            Debug.Log("Vale 0 sceneContinue");
            SceneManager.LoadScene("MenuPrincipal");
        }

    }

    public void ExitButton()
    {
        Debug.Log("Cerramos el juego");
        Application.Quit();
    }
}
