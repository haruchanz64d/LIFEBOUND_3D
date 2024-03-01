using UnityEngine;
using UnityEngine.UI;

namespace LB.Character.Interaction
{
    public class LBInteractionManager : MonoBehaviour
    {
        [SerializeField] private GameObject interactionPanel;

        private void Awake()
        {
            interactionPanel.SetActive(false);
        }
        public void ShowInteractionPanel()
        {
            interactionPanel.SetActive(true);
        }

        public void HideInteractionPanel()
        {
            interactionPanel.SetActive(false);
        }
    }
}