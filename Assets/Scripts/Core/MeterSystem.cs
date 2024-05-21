using TMPro;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public class MeterSystem : MonoBehaviour
    {
        [SerializeField] private TMP_Text distanceText;
        private Transform portalTransform;

        private void Start()
        {
            GameObject portal = GameObject.FindGameObjectWithTag("Portal");
            if (portal != null)
            {
                portalTransform = portal.transform;
            }

            distanceText.gameObject.SetActive(false);
        }

        public void UpdateDistance(float distance)
        {
            if (distanceText != null)
            {
                distanceText.text = $"Distance from the Portal: {distance:F1} meters";
            }
        }

        private void Update()
        {
            if (portalTransform != null && portalTransform.gameObject.activeSelf)
            {
                float distance = Vector3.Distance(transform.position, portalTransform.position);
                UpdateDistance(distance);

                if (!distanceText.gameObject.activeSelf)
                {
                    distanceText.gameObject.SetActive(true);
                }
            }
            else
            {
                if (distanceText.gameObject.activeSelf)
                {
                    distanceText.gameObject.SetActive(false);
                }
            }
        }
    }
}
