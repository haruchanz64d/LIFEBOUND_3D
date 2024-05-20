using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{   
    [SerializeField] private AudioClip hoverButton;
    [SerializeField] private AudioClip pressedButton;

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void Hovered()
    {
        SfxManager.instance.PlayAudioClip(hoverButton, transform, 1f);
    }

    public void Pressed()
    {
        SfxManager.instance.PlayAudioClip(pressedButton, transform, 1f);
    }
}