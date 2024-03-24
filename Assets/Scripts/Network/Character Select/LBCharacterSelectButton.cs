using UnityEngine.UI;
using UnityEngine;

public class LBCharacterSelectButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Button button;
    [SerializeField] private GameObject disabledOverlay;
    private LBCharacterSelectDisplay characterSelected;
    public LBCharacter Character { get; private set; }
    public bool IsDisabled { get; private set; }
    public void SetCharacter(LBCharacterSelectDisplay characterSelected, LBCharacter character)
    {
        iconImage.sprite = character.Icon;
        this.characterSelected = characterSelected;
        Character = character;
    }

    public void SelectCharacter()
    {
        characterSelected.Select(Character);
    }

    public void SetDisabled()
    {
        IsDisabled = true;
        disabledOverlay.SetActive(true);
        button.interactable = false;
    }
}
