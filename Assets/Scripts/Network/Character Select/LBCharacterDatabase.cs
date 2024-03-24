using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName ="New Character Database", menuName = "Character/New Character Database")]
public class LBCharacterDatabase : ScriptableObject
{
    [SerializeField] private LBCharacter[] characters = new LBCharacter[0];

    public LBCharacter[] GetLBCharacters() => characters;

    public LBCharacter GetCharacterById(int id)
    {
        foreach (var character in characters)
        {
            if (character.Id == id)
            {
                return character;
            }
        }
        return null;
    }

    public bool IsValidCharacterId(int id)
    {
        return characters.Any(x => x.Id == id);
    }
}