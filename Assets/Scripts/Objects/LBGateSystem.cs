using Assets.Scripts.Core;
using Assets.Scripts.Managers;
using UnityEngine;

public enum GateVariants
{
    SolGate,
    LunaGate,
    BothGates   
}

public class LBGateSystem : MonoBehaviour
{
    [Header("Gate Models")]
    [SerializeField] private GameObject unactivatedGate;
    [SerializeField] private GameObject activatedGate;
    [Space]
    [Header("Plate Models")]
    [SerializeField] private GameObject unactivatedPlate;
    [SerializeField] private GameObject activatedPlate;

    [SerializeField] private GateVariants gateType;

    [Header("Audio")]
    [SerializeField] private AudioClip gateActivatedClip;

    private bool isActivated = false;

    private void Start()
    {
        unactivatedGate.SetActive(true);
        activatedGate.SetActive(false);
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
                if (role.GetCharacterName() == CharacterName.Sol && gateType == GateVariants.SolGate)
                {
                    ActivateGate();
                }
                else if (role.GetCharacterName() == CharacterName.Luna && gateType == GateVariants.LunaGate)
                {
                    ActivateGate();
                }
                else if (gateType == GateVariants.BothGates)
                {
                    ActivateGate();
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

    private void ActivateGate()
    {
        if (!isActivated)
        {
            LBAudioManager.Instance.PlaySound(gateActivatedClip);
            unactivatedGate.SetActive(false);
            activatedGate.SetActive(true);
            unactivatedPlate.SetActive(false);
            activatedPlate.SetActive(true);
            isActivated = true;
        }
    }
}
