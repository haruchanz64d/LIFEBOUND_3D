using UnityEngine;
[CreateAssetMenu(fileName ="New Character", menuName="Character Selection/Character")]
public class LBCharacter : ScriptableObject
{
    [SerializeField] private GameObject previewPrefab = default;
    [SerializeField] private GameObject gameplayPrefab = default;
    public GameObject PreviewPrefab => previewPrefab;
    public GameObject GameplayPrefab => gameplayPrefab;
}
