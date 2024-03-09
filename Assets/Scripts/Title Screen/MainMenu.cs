using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject gameCreationMenu;
    [SerializeField] private GameObject optionMenu;
    [SerializeField] private GameObject titleMenu;
    [SerializeField] private GameObject controlMenu;
    [SerializeField] private GameObject audioMenu;
    [SerializeField] private GameObject keyboardVisual;
    [SerializeField] private GameObject xboxVisual;
    [SerializeField] private GameObject ps4Visual;
    [SerializeField] private GameObject controllers;

    private void Awake(){
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Start()
    {
        titleMenu.SetActive(true);
        gameCreationMenu.SetActive(false);
        optionMenu.SetActive(false);
    }

    public void OnGameCreate()
    {
        titleMenu.SetActive(false);
        gameCreationMenu.SetActive(true);
        optionMenu.SetActive(false);
    }
    public void GoToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitApp()
    {
        Application.Quit();
        Debug.Log("Quit Succesfull");
    }

    // OPTIONS
    public void Back()
    {
        optionMenu.SetActive(false);
        titleMenu.SetActive(true);
        xboxVisual.SetActive(false);
        ps4Visual.SetActive(false);
    }

    public void Options()
    {
        optionMenu.SetActive(true);
        titleMenu.SetActive(false);
        Controls();
    }

    public void Controls()
    {
        controlMenu.SetActive(true);
        keyboardVisual.SetActive(true);
        audioMenu.SetActive(false);
        controllers.SetActive(false);
    }

    public void AudioMenu()
    {
        audioMenu.SetActive(true);
        controlMenu.SetActive(false);
    }

    public void Controllers()
    {
        controllers.SetActive(true);
        Xbox();
    }

    // CONTROLLERS
    public void Keyboard()
    {
        keyboardVisual.SetActive(true);
        xboxVisual.SetActive(false);
        ps4Visual.SetActive(false);
        controllers.SetActive(false);
    }

    public void Xbox()
    {
        keyboardVisual.SetActive(false);
        xboxVisual.SetActive(true);
        ps4Visual.SetActive(false);
    }

    public void Ps4()
    {
        keyboardVisual.SetActive(false);
        xboxVisual.SetActive(false);
        ps4Visual.SetActive(true);
    }

}
