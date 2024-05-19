using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class AmbientMusicTrigger : MonoBehaviour
    {
        private AudioSource audio;

        private void Start()
        {
            audio = GetComponent<AudioSource>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                audio.Play();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                audio.Stop();
            }
        }
    }
}