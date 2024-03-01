using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
public class LBRoleAssigner : NetworkBehaviour
{
    [Header("Models")]
    [SerializeField] private GameObject solModel;
    [SerializeField] private GameObject lunaModel;
    [Space(5)]
    [Header("Minimap Icons")]
    [SerializeField] private SpriteRenderer solMinimapIcon;
    [SerializeField] private SpriteRenderer lunaMinimapIcon;
    private string SOL_MODEL = "Sol"; // 1P
    private string LUNA_MODEL = "Luna"; // 2P
    private string ASSIGNED_ROLE;
    public string GET_ASSIGNED_ROLE() { return ASSIGNED_ROLE; }
    public string GET_SOL_ROLE() { return SOL_MODEL; }
    public string GET_LUNA_ROLE() { return LUNA_MODEL; }

    private void Start()
    {
        if (NetworkManager.Singleton.IsHost) AssignHostRoleAndModel();
        else if (NetworkManager.Singleton.IsClient) AssignClientRoleAndModel();
    }

    private void AssignHostRoleAndModel()
    {
        ASSIGNED_ROLE = GET_SOL_ROLE();
        // ASSIGN MODEL
        solModel.SetActive(true);
        lunaModel.SetActive(false);

        // ASSIGN MINIMAP ICON
        solMinimapIcon.enabled = true;
        lunaMinimapIcon.enabled = false;
        Debug.Log($"Assigned role is {ASSIGNED_ROLE}");
    }

    private void AssignClientRoleAndModel()
    {
        ASSIGNED_ROLE = GET_LUNA_ROLE();
        // ASSIGN MODEL
        solModel.SetActive(false);
        lunaModel.SetActive(true);

        // ASSIGN MINIMAP ICON
        solMinimapIcon.enabled = false;
        lunaMinimapIcon.enabled = true;
        Debug.Log($"Assigned role is {ASSIGNED_ROLE}");
    }
}
