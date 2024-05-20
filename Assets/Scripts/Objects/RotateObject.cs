using System.Collections;
using System.Collections.Generic;
using LB.Character;
using Unity.Netcode;
using UnityEngine;

public class RotateObject : NetworkBehaviour
{
    private float rotationSpeed = 50f;

    private void Update()
    {
        ApplyRotationServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ApplyRotationServerRpc()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
