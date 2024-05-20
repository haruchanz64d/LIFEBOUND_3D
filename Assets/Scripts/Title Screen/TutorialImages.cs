using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialImages : MonoBehaviour
{
    [SerializeField] Sprite[] images;
    [SerializeField] Image imageComponent;
    [SerializeField] GameObject nextButton;
    [SerializeField] GameObject previousButton;
    private int currentIndex = 0;

    void Start()
    {   
        if (images.Length > 0 && imageComponent != null)
        {
            imageComponent.sprite = images[currentIndex];
        }

        previousButton.SetActive(false);
    }

    public void NextImage()
    {
        currentIndex = (currentIndex + 1) % images.Length;
        imageComponent.sprite = images[currentIndex];

        if (currentIndex == images.Length - 1)
        {
            nextButton.SetActive(false);
        }

        previousButton.SetActive(true);
    }

    public void PreviousImage()
    {
        currentIndex = (currentIndex - 1 + images.Length) % images.Length;
        imageComponent.sprite = images[currentIndex];

        if (currentIndex == 0)
        {
            previousButton.SetActive(false);
        }

        nextButton.SetActive(true);
    }
}
