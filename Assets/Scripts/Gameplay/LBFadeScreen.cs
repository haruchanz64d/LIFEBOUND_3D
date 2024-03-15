/// <summary>
/// Manages the fading of an image component.
/// Note: Unused in the meantime
/// </summary>
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LBFadeScreen : MonoBehaviour
{
    /// <summary>
    /// The image to be faded.
    /// </summary>
    public Image imageToFade;

    /// <summary>
    /// The duration of the fade animation.
    /// </summary>
    public float fadeTime = 1.0f;

    /// <summary>
    /// Initializes the image as disabled on start.
    /// </summary>
    void Start()
    {
        imageToFade.enabled = false;
    }

    /// <summary>
    /// Fades the image in or out based on the specified parameter.
    /// </summary>
    /// <param name="fadeIn">Flag to determine if the fade is in (true) or out (false).</param>
    public IEnumerator FadeImage(bool fadeIn)
    {
        imageToFade.enabled = true;
        float targetAlpha;
        float startAlpha = imageToFade.color.a;

        if (fadeIn)
        {
            targetAlpha = 1.0f;
        }
        else
        {
            targetAlpha = 0.0f;
        }

        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / fadeTime)
        {
            imageToFade.color = new Color(imageToFade.color.r, imageToFade.color.g, imageToFade.color.b, Mathf.Lerp(startAlpha, targetAlpha, t));
            yield return null;
        }
        imageToFade.color = new Color(imageToFade.color.r, imageToFade.color.g, imageToFade.color.b, targetAlpha);
    
        imageToFade.enabled = false;
    }
}
