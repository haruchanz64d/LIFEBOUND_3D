using UnityEngine;

public class LBCharacterSelect : MonoBehaviour
{
    [SerializeField] private GameObject characterSelectedDisplay = default;
    [SerializeField] private Transform[] characterPreviewParents = default;

    [SerializeField] private float turnSpeed = 90f;

    private void Update()
    {
        for(int x = 0; x < characterPreviewParents.Length; x++)
        {
            characterPreviewParents[x].RotateAround(characterPreviewParents[x].position, characterPreviewParents[x].up, turnSpeed * Time.deltaTime);
        }
    }
}