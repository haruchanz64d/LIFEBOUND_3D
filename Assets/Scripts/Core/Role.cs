using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Core
{
    public enum CharacterName
    {
        Sol,
        Luna
    }

    public class Role : NetworkBehaviour
    {
        [Header("Character Name")]
        [SerializeField] private CharacterName characterName;
        private CharacterName originalCharacterName;
        [Space]
        [Header("Character Model")]
        [SerializeField] private GameObject solModel;
        [SerializeField] private GameObject lunaModel;
        [Header("UI")]
        [SerializeField] private SpriteRenderer characterMinimapIcon;
        [SerializeField] private Sprite[] characterMinimapIcons;

        private void Awake()
        {
            SetCharacterModel(characterName);
            Debug.Log("Character Name: " + characterName);
        }

        public void SetCharacterName(CharacterName name)
        {
            characterName = name;
            originalCharacterName = name;
        }

        public CharacterName GetCharacterName()
        {
            return characterName;
        }

        public void SetCharacterModel(CharacterName name)
        {
            switch (name)
            {
                case CharacterName.Sol:
                    solModel.SetActive(true);
                    lunaModel.SetActive(false);
                    characterMinimapIcon.sprite = characterMinimapIcons[0];
                    break;
                case CharacterName.Luna:
                    solModel.SetActive(false);
                    lunaModel.SetActive(true);
                    characterMinimapIcon.sprite = characterMinimapIcons[1];
                    break;
            }
        }
        public void SwapCharacterModel()
        {
            switch (characterName)
            {
                case CharacterName.Sol:
                    solModel.SetActive(false);
                    lunaModel.SetActive(true);
                    characterName = CharacterName.Luna;
                    break;
                case CharacterName.Luna:
                    solModel.SetActive(true);
                    lunaModel.SetActive(false);
                    characterName = CharacterName.Sol;
                    break;
            }
        }

        public void ResetCharacterModel()
        {
            SetCharacterModel(originalCharacterName);
            characterName = originalCharacterName;
        }

        [ClientRpc]
        public void ResetCharacterModelClientRpc()
        {
            ResetCharacterModel();
        }
    }
}
