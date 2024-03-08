using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LBFadeScreen : MonoBehaviour
{
    public Image imageToFade;
    public float fadeTime = 1.0f;

    private bool isFadingIn = false;

    private void Start()
    {
        imageToFade.enabled = false;
    }

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
