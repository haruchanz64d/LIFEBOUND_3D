using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class LBAudioManager : MonoBehaviour
    {
        public static LBAudioManager Instance { get; private set; }

        private AudioSource audioSource;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                audioSource = GetComponent<AudioSource>();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlaySound(AudioClip clip)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}