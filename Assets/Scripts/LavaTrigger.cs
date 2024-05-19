using UnityEngine;
using Unity.Netcode;

public class LavaTrigger : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.KillAllPlayersDueToLavaServerRpc();
        }
    }
}
