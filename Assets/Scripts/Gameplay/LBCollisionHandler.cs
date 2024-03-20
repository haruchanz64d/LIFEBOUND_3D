using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class LBCollisionHandler : MonoBehaviour
{
    private bool isPlayerDead = false;
    public bool IsPlayerDead { get { return isPlayerDead; } set { isPlayerDead = value; } }
    private Animator animator;
    [SerializeField] private Image lBFadeScreen;
    private Transform originalSpawnPoint;
    private Transform checkpointSpawnPoint;

    private Vector3 lastCheckpointInteracted;

    private void Awake()
    {
        originalSpawnPoint = GameObject.FindGameObjectWithTag("Spawn Point").transform;
        animator = GetComponent<Animator>();
    }

    private void OnTriggerStay(Collider hit)
    {
        if (hit.gameObject.CompareTag("Lava"))
        {
            animator.Play("Death");
            isPlayerDead = true;
            StartCoroutine(RespawnPlayer());
        }
    }
    public IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(5f);
        // temporarily respawn from the original spawn point
        transform.position = originalSpawnPoint.transform.position;

        Debug.Log($"Setting current position {transform.position} to last checkpoint {originalSpawnPoint.transform.position}");

        isPlayerDead = false;
        animator.SetTrigger("IsAlive");
    }
}