using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public enum CharacterName
    {
        Sol,
        Luna
    }

    public class LBRole : NetworkBehaviour
    {
        [Header("Character Name")]
        [SerializeField] private CharacterName characterName;
        private CharacterName originalCharacterName;
        [Space] 
        [Header("Character Model")]
        [SerializeField] private GameObject solModel;
        [SerializeField] private GameObject lunaModel;

        public void SetCharacterName(CharacterName name)
        {
            characterName = name;
            originalCharacterName = name;
        }

        public CharacterName GetCharacterName()
        {
            return characterName;
        }

        private void Awake()
        {
            SetCharacterModel(characterName);
        }

        public void SetCharacterModel(CharacterName name)
        {
            switch (name)
            {
                case CharacterName.Sol:
                    solModel.SetActive(true);
                    lunaModel.SetActive(false);
                    break;
                case CharacterName.Luna:
                    solModel.SetActive(false);
                    lunaModel.SetActive(true);
                    break;
            }
        }

        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        public void SwapCharacterModelRpc()
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

        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        public void ResetCharacterModelRpc()
        {
            SetCharacterModel(originalCharacterName);
            characterName = originalCharacterName;
        }
    }
}