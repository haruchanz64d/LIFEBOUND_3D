using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public enum CharacterName
    {
        Sol,
        Luna
    }

    public class LBRole : MonoBehaviour
    {
        [SerializeField] private CharacterName characterName;

        public void SetCharacterName(CharacterName name)
        {
            characterName = name;
        }

        public CharacterName GetCharacterName()
        {
            return characterName;
        }
    }
}