using UnityEngine;

public class LBSignboardSystem : MonoBehaviour
{
    [SerializeField] private SignboardData signboardData;

    [SerializeField] private string signboardTitle;
    [SerializeField] private string signboardDescription;
    [SerializeField] private Sprite signboardImage;

    public void SetSignboardData(SignboardData data)
    {
        signboardData = data;
        signboardTitle = signboardData.signboardTitle;
        signboardDescription = signboardData.signboardDescription;
        signboardImage = signboardData.signboardImage;
    }
}
