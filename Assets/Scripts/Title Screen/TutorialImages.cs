using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialImages : MonoBehaviour
{
    [SerializeField] Sprite[] images;
    [SerializeField] Image imageComponent;
    [SerializeField] GameObject nextButton;
    [SerializeField] GameObject closeButton;
    private int currentIndex = 0;

    void Start()
    {   
        nextButton.SetActive(true);
        closeButton.SetActive(false);

        if (images.Length > 0 && imageComponent != null)
        {
            imageComponent.sprite = images[currentIndex];
        }
    }

    public void NextImage()
    {
        currentIndex = (currentIndex + 1) % images.Length;
        imageComponent.sprite = images[currentIndex];

        if (currentIndex == images.Length - 1)
        {
            nextButton.SetActive(false);
            closeButton.SetActive(true);
        }
    }
}
