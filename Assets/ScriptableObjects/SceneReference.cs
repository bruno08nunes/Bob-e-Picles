using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "SceneReference", menuName = "Game/Scene Reference")]
public class SceneReference : ScriptableObject
{
    [SerializeField] private string sceneName;

    public string GetSceneName()
    {
        return sceneName;
    }
}