using UnityEngine;
using TMPro;
public class CredentialsManager : MonoBehaviour
{
    [SerializeField] private GameObject notificationCanvas;
    [SerializeField] private TMP_Text notificationText;
    [SerializeField] private TMP_InputField usernameField, passwordField;

    private void Awake()
    {
        notificationCanvas.SetActive(false);
    }

    private void Update()
    {
        passwordField.contentType = TMP_InputField.ContentType.Password;
    }
    public void LoginAccount()
    {
        if(string.IsNullOrEmpty(usernameField.text) || string.IsNullOrEmpty(passwordField.text))
        {
            ShowErrorNotification("Fields must not be empty!");
            return;
        }
    }

    private void ShowErrorNotification(string message)
    {
        notificationText.text = message;
        notificationCanvas.SetActive(true);
        Invoke(nameof(HideNotification), 3f);
    }

    private void HideNotification()
    {
        notificationCanvas.SetActive(false);
    }
}
