using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
public class LBCharacterModelSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject parent;
    [SerializeField] private GameObject[] characterModels;
    [SerializeField] private Sprite[] minimapIcons;
    [SerializeField] private Image minimapIcon;

    public override void OnNetworkSpawn()
    {
       if (IsOwner)
        {
            AssignModelandIconForHost();
        }
       else
        {
            AssignModelAndIconForClient();
        }
    }

    public void AssignModelandIconForHost()
    {
        GameObject characterModel = Instantiate(characterModels[0], parent.transform);
        minimapIcon.sprite = minimapIcons[0];
    }

    public void AssignModelAndIconForClient()
    {
        GameObject characterModel = Instantiate(characterModels[1], parent.transform);
        minimapIcon.sprite = minimapIcons[1];
    }
}