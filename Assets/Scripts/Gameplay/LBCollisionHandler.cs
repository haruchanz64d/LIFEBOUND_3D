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
    private Vector3 lastCheckpointInteracted;

    private void Awake()
    {
        originalSpawnPoint = GameObject.FindGameObjectWithTag("Spawn Point").transform;
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isPlayerDead) StartCoroutine(RespawnPlayer());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Checkpoint"))
        {
            Debug.Log($"Checkpoint located at X: {other.transform.position.x}, Y: {other.transform.position.y}, Z: {other.transform.position.z}");
            lastCheckpointInteracted = other.gameObject.transform.position;
            other.GetComponent<LBCheckpoint>().OnCheckpointActivated();
        }
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

        Vector3 respawnPosition = lastCheckpointInteracted != Vector3.zero ? lastCheckpointInteracted : originalSpawnPoint.position;
        transform.position = respawnPosition;

        if (lastCheckpointInteracted != Vector3.zero)
        {
            Debug.Log($"Respawned player at checkpoint: {lastCheckpointInteracted}");
        }
        else
        {
            Debug.Log("Respawned player at original spawn point.");
        }

        Debug.Log($"Respawned player at checkpoint: {lastCheckpointInteracted}");

        isPlayerDead = false;
        animator.SetTrigger("IsAlive");
    }
}
