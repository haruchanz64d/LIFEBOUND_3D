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

        // Set the character name to the specified value
        public void SetCharacterName(CharacterName name)
        {
            characterName = name;
        }

        // Get the character name
        public CharacterName GetCharacterName()
        {
            return characterName;
        }
    }
}