using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Managers;
using LB.Character;
using Unity.Netcode;
using UnityEngine;

public class HeartCollectible : NetworkBehaviour
{
    private float rotationSpeed = 50f;
    private GameManager gameManager;

    [SerializeField] private AudioClip collectHeartSoundEffect;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        gameManager = GameManager.Instance;
    }
    private void Update()
    {
        ApplyRotation();
    }

    private void ApplyRotation()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AudioManager.Instance.PlaySound(collectHeartSoundEffect);
            gameManager.UpdateCollectionCount();
            if(IsServer)
            {
                CollectHeartServerRpc();
            }
            else
            {
                CollectHeartClientRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CollectHeartServerRpc()
    {
        if (IsServer)
        {
            NetworkObject.Despawn();
            Destroy(gameObject);
        }
    }

    [ClientRpc]
    private void CollectHeartClientRpc()
    {
         if (IsClient)
        {
            NetworkObject.Despawn();
            Destroy(gameObject);
        }
    }
}
