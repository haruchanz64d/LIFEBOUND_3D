using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine;

public class LBSoulSwap : MonoBehaviour
{
    [SerializeField] private InputActionReference ultimateInput;
    [SerializeField] private Image soulSwapIcon;
    private Animator animator;
    [SerializeField] private GameObject[] playerModels;
    private LBGameManager instance;
}