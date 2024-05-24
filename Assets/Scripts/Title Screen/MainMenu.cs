using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject gameSelectionCanvas;
    [SerializeField] private GameObject optionMenu;
    [SerializeField] private GameObject titleMenu;
    [SerializeField] private GameObject controlMenu;
    [SerializeField] private GameObject audioMenu;
    [SerializeField] private AudioClip hoverButton;
    [SerializeField] private AudioClip pressedButton;

    private void Awake(){
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Start()
    {
        titleMenu.SetActive(true);
        optionMenu.SetActive(false);
        gameSelectionCanvas.SetActive(false);
    }

    private void LateUpdate()
    {
        //OnLoadDebugRoom();
    }

    public void OnPlaySelected(string sceneName)
    {
        StartCoroutine(LoadingScreenBeforeSceneLoad(sceneName));
    }

    // please create a UI for confirmation that the player wants to quit instead of quitting :)
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
        audioMenu.SetActive(false);
    }

    public void AudioMenu()
    {
        audioMenu.SetActive(true);
        controlMenu.SetActive(false);
    }

    private IEnumerator LoadingScreenBeforeSceneLoad(string sceneName)
    {

        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);

        while (!async.isDone)
        {
            yield return null;
        }
    }

    // BUTTON SOUNDS
    public void Hovered()
    {
        SfxManager.instance.PlayAudioClip(hoverButton, transform, 1f);
    }

    public void Pressed()
    {
        SfxManager.instance.PlayAudioClip(pressedButton, transform, 1f);
    }
}
