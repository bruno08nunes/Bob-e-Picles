using Unity.Netcode;
using UnityEngine;

public class SceneController : NetworkBehaviour
{
    [SerializeField] private SceneReference[] scenes = new SceneReference[25];

    public static SceneController Instance { get; private set; }

    public int CurrentScene { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentScene = 12;
    }

    private static void LoadScene(int currentScene)
    {
        if (Instance.scenes[currentScene] == null)
        {
            Debug.LogError("Scene not created");
            return;
        }

        NetworkManager.Singleton.SceneManager.LoadScene(Instance.scenes[currentScene].GetSceneName(), UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public static void GoToUpScene()
    {
        if (Instance.CurrentScene - 5 < 0)
        {
            Debug.LogError("Impossible Scene");
            return;
        }

        Instance.CurrentScene -= 5;
        LoadScene(Instance.CurrentScene);
    }

    public static void GoToLeftScene()
    {
        if (Instance.CurrentScene % 5 < 0)
        {
            Debug.LogError("Impossible Scene");
            return;
        }

        Instance.CurrentScene -= 1;
        LoadScene(Instance.CurrentScene);
    }

    public static void GoToRightScene()
    {
        if (Instance.CurrentScene % 5 > 4)
        {
            Debug.LogError("Impossible Scene");
            return;
        }

        Instance.CurrentScene += 1;
        LoadScene(Instance.CurrentScene);
    }

    public static void GoToDownScene()
    {
        if (Instance.CurrentScene + 5 > 24)
        {
            Debug.LogError("Impossible Scene");
            return;
        }

        Instance.CurrentScene += 5;
        LoadScene(Instance.CurrentScene);
    }
}
