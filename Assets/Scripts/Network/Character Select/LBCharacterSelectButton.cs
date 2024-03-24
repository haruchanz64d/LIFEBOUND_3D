using UnityEngine.UI;
using UnityEngine;

public class LBCharacterSelectButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    private LBCharacterSelectDisplay characterSelected;
    private LBCharacter character;

    public void SetCharacter(LBCharacterSelectDisplay characterSelected, LBCharacter character)
    {
        iconImage.sprite = character.Icon;
        this.characterSelected = characterSelected;
        this.character = character;
    }

    public void SelectCharacter()
    {
        characterSelected.Select(character);
    }
}
