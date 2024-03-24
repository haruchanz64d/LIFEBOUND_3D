using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class LBPlayerCard : MonoBehaviour
{
    [SerializeField] private LBCharacterDatabase characterDatabase;
    [SerializeField] private GameObject visuals;
    [Header("Visuals")]
    [SerializeField] private Image characterIconImage;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text characterNameText;

    public void UpdateDisplay(LBCharacterSelectState state)
    {
        if (state.CharacterId != -1)
        {
            var character = characterDatabase.GetCharacterById(state.CharacterId);
            characterIconImage.sprite = character.Icon;
            characterIconImage.enabled = true;
            characterNameText.SetText(character.DisplayName);
        }
        else
        {
            characterIconImage.enabled = false;
        }

        //playerNameText.text = $"Player {state.ClientId}";

        visuals.SetActive(true);
    }

    public void DisableDisplay()
    {
        visuals.SetActive(false);
    }
}
