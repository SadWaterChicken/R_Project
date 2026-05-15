using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    

    public void PlayGame()
    {
        // Load the game scene
        SceneManager.LoadScene("Testing");
    }

    public void QuitGame()
    {
        // Quit the application
        Application.Quit();
    }
}

