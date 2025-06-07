using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ESceneType
{
    Title,
    Lobby,
    InGame,
}

public class SceneLoader : SingletonBehaviour<SceneLoader>
{
    public void LoadScene(ESceneType sceneType)
    {
        Logger.Log($"{sceneType} scene loading...");

        Time.timeScale = 1f;
        UIManager.Instance.CloseAllOpenUI();
        SceneManager.LoadScene(sceneType.ToString());
    }

    public void ReloadScene()
    {
        Logger.Log($"{SceneManager.GetActiveScene().name} scene loading...");

        Time.timeScale = 1f;
        UIManager.Instance.CloseAllOpenUI();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public AsyncOperation LoadSceneAsync(ESceneType sceneType)
    {
        Logger.Log($"{sceneType} scene async loading...");

        Time.timeScale = 1f;
        UIManager.Instance.CloseAllOpenUI();
        return SceneManager.LoadSceneAsync(sceneType.ToString());
    }

    public ESceneType GetCurrentScene()
    {
        return (ESceneType)Enum.Parse(typeof(ESceneType), SceneManager.GetActiveScene().name);
    }
}
