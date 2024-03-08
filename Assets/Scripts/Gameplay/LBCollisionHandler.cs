using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class LBCollisionHandler : MonoBehaviour
{
    private bool isPlayerDead = false;
    public bool IsPlayerDead { get { return isPlayerDead; } set { isPlayerDead = value; } }
    private Animator animator;
    [SerializeField] private LBFadeScreen lBFadeScreen;
    private Transform originalSpawnPoint;
    private Transform checkpointSpawnPoint; // UNUSED

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
        }
    }
    private IEnumerator Respawn()
    {
        yield return null;
    }
}