using UnityEngine;
using System.Collections;

namespace LB.Environment.Objects
{
    public class LBFallingPlatform : MonoBehaviour
    {
        [SerializeField] private GameObject fallingPlatformPrefab;
        private bool isFalling = false;
        [SerializeField] private float fallSpeed = 0.5f; 
        [SerializeField] private Transform initialPosition;
        private float fallStartTime;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                isFalling = true;
                fallStartTime = Time.time;
                StartCoroutine(FallAndRespawn());
            }
        }

        private void Update()
        {
            if (isFalling)
            {
                float elapsedTime = Time.time - fallStartTime;

                transform.position = new Vector3(transform.position.x, transform.position.y - (fallSpeed * elapsedTime), transform.position.z);
            }
        }

        private IEnumerator FallAndRespawn()
        {
            yield return new WaitForSeconds(5f);
            Destroy(gameObject);
        }
    }
}
