using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class LBCharacterSelectionManager : MonoBehaviour
{
    [Header("Scenes To Load")]
    [SerializeField] private SceneField gameplayScene;
    [SerializeField] private SceneField characterSelectScene;

    private List<AsyncOperation> sceneToLoad = new List<AsyncOperation>();

    public void Awake()
    {
        sceneToLoad.Add(SceneManager.LoadSceneAsync(gameplayScene, LoadSceneMode.Additive));
    }

    public void OnCharacterSelected()
    {
        sceneToLoad.Add(SceneManager.UnloadSceneAsync(characterSelectScene, (UnloadSceneOptions)LoadSceneMode.Additive));
    }
}
