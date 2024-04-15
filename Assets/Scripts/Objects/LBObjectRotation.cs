using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LBObjectRotation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 100f;

    private void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}
