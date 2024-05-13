using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource m_audioSource;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                m_audioSource = GetComponent<AudioSource>();
            }
            else
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(this);
        }

        /// <summary>
        /// Plays a sound
        /// </summary>
        /// <param name="clip">The audio clip of the sound to be played.</param>
        public void PlaySound(AudioClip clip)
        {
            m_audioSource.PlayOneShot(clip);
        }
    }
}