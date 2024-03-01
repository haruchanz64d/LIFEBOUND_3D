using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LBGateChecker : MonoBehaviour
{
    private LBRoleAssigner role;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            
        }
    }
}
