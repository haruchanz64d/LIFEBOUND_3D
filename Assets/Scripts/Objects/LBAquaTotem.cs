using System.Collections;
using System.Collections.Generic;
using LB.Character;
using UnityEngine;

public class LBAquaTotem : MonoBehaviour
{
    [SerializeField] private int healingAmount = 2;
    [SerializeField] private float healingInterval = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.Heal(healingAmount);
                StartCoroutine(HealingCoroutine(player));
            }
        }
    }

    IEnumerator HealingCoroutine(Player player)
    {
        while (true)
        {
            player.Heal(healingAmount);
            yield return new WaitForSeconds(healingInterval);
        }
    }
}
