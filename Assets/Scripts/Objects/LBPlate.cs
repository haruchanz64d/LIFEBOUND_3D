using Assets.Scripts.Core;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    public enum PlateVariants
    {
        SolPlate,
        LunaPlate,
        BothPlates
    }
    public class LBPlate : MonoBehaviour
    {
        [Header("Plate Models")]
        [SerializeField] private GameObject unactivatedPlate;
        [SerializeField] private GameObject activatedPlate;

        [SerializeField] private PlateVariants plateType;
        private bool isActivated = false;

        private void Start()
        {
            unactivatedPlate.SetActive(true);
            activatedPlate.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (other.transform.position.y > transform.position.y)
                {
                    LBRole role = other.GetComponent<LBRole>();
                    if (role.GetCharacterName() == CharacterName.Sol && plateType == PlateVariants.SolPlate)
                    {
                        ActivatePlate();
                    }
                    else if (role.GetCharacterName() == CharacterName.Luna && plateType == PlateVariants.LunaPlate)
                    {
                        ActivatePlate();
                    }
                    else if (plateType == PlateVariants.BothPlates)
                    {
                        ActivatePlate();
                    }
                }
            }
        }
        
        private void ActivatePlate()
        {
            if (!isActivated)
            {
                unactivatedPlate.SetActive(false);
                activatedPlate.SetActive(true);
                isActivated = true;
            }
        }
    }
}