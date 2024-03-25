using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName ="New Character", menuName ="Character/New Character")]
public class LBCharacter : ScriptableObject
{
    [SerializeField] private int id = -1;
    [SerializeField] private string displayName = "New Display Name";
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject introCharacter;
    [SerializeField] private NetworkObject gameplayPrefab;

    public int Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public GameObject IntroCharacter => introCharacter;
    public NetworkObject GameplayPrefab => gameplayPrefab;
}
