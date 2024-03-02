using UnityEngine;
using TMPro;
using Mirror;
using System.Collections.Generic;

public class LBCharacterSelect : NetworkBehaviour
{
    [SerializeField] private GameObject characterSelectedDisplay = default;
    [SerializeField] private Transform characterPreviewParent = default;
    [SerializeField] private TMP_Text characterSelectName = default;
    [SerializeField] private float turnSpeed = 90f;
    [SerializeField] private LBCharacter[] lBCharacters = default;

    private int currentIndex = 0;
    private List<GameObject> characterInstances = new List<GameObject>();

    public override void OnStartClient()
    {
        if (characterPreviewParent.childCount == 0)
        {
            foreach (var character in lBCharacters)
            {
                GameObject characterInstance =
                Instantiate(character.PreviewPrefab, characterPreviewParent);

                characterInstance.SetActive(false);

                characterInstances.Add(characterInstance);
            }
        }

        characterInstances[currentIndex].SetActive(true);
        characterSelectName.text = lBCharacters[currentIndex].CharacterName;

        characterSelectedDisplay.SetActive(true);
    }

    private void Update()
    {
        characterPreviewParent.RotateAround(characterPreviewParent.position, characterPreviewParent.up, turnSpeed * Time.deltaTime);
    }

    public void SelectCharacter()
    {
        CmdSelect(currentIndex);
        characterSelectedDisplay.SetActive(false);
    }

    public void CmdSelect(int currentIndex, NetworkConnectionToClient sender = null)
    {
        GameObject characterInstance = Instantiate(lBCharacters[currentIndex].GameplayPrefab);
        NetworkServer.Spawn(characterInstance, sender);
    }

    public void SelectLeft()
    {
        characterInstances[currentIndex].SetActive(false);

        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex += characterInstances.Count;
        }

        characterInstances[currentIndex].SetActive(true);
        characterSelectName.text = lBCharacters[currentIndex].CharacterName;
    }

    public void SelectRight()
    {
        characterInstances[currentIndex].SetActive(false);

        currentIndex = (currentIndex + 1) % characterInstances.Count;

        characterInstances[currentIndex].SetActive(true);
        characterSelectName.text = lBCharacters[currentIndex].CharacterName;
    }
}
