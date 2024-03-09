using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LBCharacterSelector : MonoBehaviour
{
    public void PlayAsSol(GameObject solPrefab)
    {
        LoadGameplayScene(solPrefab);
    }

    public void PlayAsLuna(GameObject lunaPrefab)
    {
        LoadGameplayScene(lunaPrefab);
    }

    public void LoadGameplayScene(GameObject selectedCharacter)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Gameplay Scene", LoadSceneMode.Single);
        asyncLoad.completed += OnSceneLoaded;

        void OnSceneLoaded(AsyncOperation asyncLoad)
        {
            Instantiate(selectedCharacter, GameObject.FindGameObjectWithTag("Spawn Point").transform.position, Quaternion.identity);
        }
    }

}