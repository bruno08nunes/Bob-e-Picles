
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : NetworkBehaviour
{
    [Header("Scenes")]
    [SerializeField] private SceneReference[] scenes = new SceneReference[25];

    [Header("Scene Change")]
    [SerializeField] private float sceneChangeCooldown = 0.4f;

    [Header("Screen Transition")]
    [SerializeField] private CanvasGroup transitionCanvasGroup;
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private float fadeOutTimeout = 5f;

    public static SceneController Instance { get; private set; }

    public int CurrentScene { get; private set; }

    private string direction = "";

    private float nextSceneChangeTime;
    private bool isChangingScene;

    private readonly HashSet<ulong> fadeOutCompletedClients = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentScene = 12;

        if (transitionCanvasGroup != null)
        {
            transitionCanvasGroup.alpha = 0f;
            transitionCanvasGroup.interactable = false;
            transitionCanvasGroup.blocksRaycasts = false;

            Transform canvasRoot = transitionCanvasGroup.transform.root;

            if (canvasRoot != transform.root)
            {
                DontDestroyOnLoad(canvasRoot.gameObject);
            }
        }
        else
        {
            Debug.LogError("Transition Canvas Group is not assigned!");
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SetPlayersPosition;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer)
            return;

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SetPlayersPosition;
    }

    private void SetPlayersPosition(
        string sceneName,
        LoadSceneMode loadSceneMode,
        List<ulong> clientsCompleted,
        List<ulong> clientsTimedOut)
    {
        if (!IsServer || direction == "")
            return;

        PlayerMove[] players = FindObjectsOfType<PlayerMove>();

        Vector3[] positions = new Vector3[2];

        switch (direction)
        {
            case "Left":
                positions[0] = new Vector3(7, -1, 0);
                positions[1] = new Vector3(7, 1, 0);
                break;

            case "Right":
                positions[0] = new Vector3(-7, -1, 0);
                positions[1] = new Vector3(-7, 1, 0);
                break;

            case "Up":
                positions[0] = new Vector3(-1, -4, 0);
                positions[1] = new Vector3(1, -4, 0);
                break;

            case "Down":
                positions[0] = new Vector3(-1, 4, 0);
                positions[1] = new Vector3(1, 4, 0);
                break;
        }

        for (int i = 0; i < 2; i++)
        {
            players[i].SetPosition(positions[i]);
        }

        direction = "";

        FadeInClientRpc();

        StartCoroutine(FadeCanvas(0f));

        isChangingScene = false;
    }

    private static bool CanChangeScene()
    {
        if (Instance == null)
            return false;

        if (Instance.isChangingScene)
            return false;

        if (Time.time < Instance.nextSceneChangeTime)
            return false;

        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsServer)
            return false;

        return true;
    }

    private static void LoadScene(int targetScene, string newDirection)
    {
        if (!CanChangeScene())
            return;

        if (targetScene < 0 || targetScene >= Instance.scenes.Length)
        {
            Debug.LogError("Impossible Scene");
            return;
        }

        if (Instance.scenes[targetScene] == null)
        {
            Debug.LogError("Scene not created");
            return;
        }

        Instance.isChangingScene = true;

        Instance.StartCoroutine(
            Instance.BeginSceneChange(targetScene, newDirection)
        );
    }

    private IEnumerator BeginSceneChange(int targetScene, string newDirection)
    {
        fadeOutCompletedClients.Clear();

        // Start fading out on remote clients.
        FadeOutClientRpc();

        // Fade out on the server.
        yield return StartCoroutine(FadeCanvas(1f));

        // The server has completed its fade.
        fadeOutCompletedClients.Add(NetworkManager.ServerClientId);

        // Wait until all connected clients have completed their fade.
        float timeout = Time.realtimeSinceStartup + fadeOutTimeout;

        while (!AllClientsCompletedFadeOut() &&
               Time.realtimeSinceStartup < timeout)
        {
            yield return null;
        }

        if (!AllClientsCompletedFadeOut())
        {
            Debug.LogWarning("Fade out timed out. Loading scene anyway.");
        }

        string sceneName = scenes[targetScene].GetSceneName();

        var status = NetworkManager.Singleton.SceneManager.LoadScene(
            sceneName,
            LoadSceneMode.Single
        );

        if (status != SceneEventProgressStatus.Started)
        {
            Debug.LogError($"Failed to load scene: {status}");

            direction = "";
            isChangingScene = false;

            FadeInClientRpc();
            StartCoroutine(FadeCanvas(0f));

            yield break;
        }

        CurrentScene = targetScene;
        direction = newDirection;

        nextSceneChangeTime = Time.time + sceneChangeCooldown;
    }

    private bool AllClientsCompletedFadeOut()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!fadeOutCompletedClients.Contains(clientId))
                return false;
        }

        return true;
    }

    [Rpc(SendTo.NotServer)]
    private void FadeOutClientRpc()
    {
        StartCoroutine(FadeOutAndNotifyServer());
    }

    private IEnumerator FadeOutAndNotifyServer()
    {
        yield return StartCoroutine(FadeCanvas(1f));

        FadeOutCompletedServerRpc();
    }

    [Rpc(SendTo.Server)]
    private void FadeOutCompletedServerRpc(RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        fadeOutCompletedClients.Add(clientId);
    }

    [Rpc(SendTo.NotServer)]
    private void FadeInClientRpc()
    {
        StartCoroutine(FadeCanvas(0f));
    }

    private IEnumerator FadeCanvas(float targetAlpha)
    {
        if (transitionCanvasGroup == null)
            yield break;

        transitionCanvasGroup.interactable = false;
        transitionCanvasGroup.blocksRaycasts = true;

        float startAlpha = transitionCanvasGroup.alpha;
        float elapsedTime = 0f;

        if (fadeDuration <= 0f)
        {
            transitionCanvasGroup.alpha = targetAlpha;
        }
        else
        {
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;

                float progress = Mathf.Clamp01(elapsedTime / fadeDuration);

                transitionCanvasGroup.alpha = Mathf.Lerp(
                    startAlpha,
                    targetAlpha,
                    progress
                );

                yield return null;
            }

            transitionCanvasGroup.alpha = targetAlpha;
        }

        if (targetAlpha <= 0f)
        {
            transitionCanvasGroup.blocksRaycasts = false;
        }
    }

    public static void GoToUpScene()
    {
        if (Instance == null)
            return;

        int targetScene = Instance.CurrentScene - 5;

        if (targetScene < 0)
        {
            Debug.LogError("Impossible Scene");
            return;
        }

        LoadScene(targetScene, "Up");
    }

    public static void GoToLeftScene()
    {
        if (Instance == null)
            return;

        int targetScene = Instance.CurrentScene - 1;

        if (targetScene < 0 || Instance.CurrentScene % 5 == 0)
        {
            Debug.LogError("Impossible Scene");
            return;
        }

        LoadScene(targetScene, "Left");
    }

    public static void GoToRightScene()
    {
        if (Instance == null)
            return;

        int targetScene = Instance.CurrentScene + 1;

        if (Instance.CurrentScene % 5 == 4)
        {
            Debug.LogError("Impossible Scene");
            return;
        }

        LoadScene(targetScene, "Right");
    }

    public static void GoToDownScene()
    {
        if (Instance == null)
            return;

        int targetScene = Instance.CurrentScene + 5;

        if (targetScene > 24)
        {
            Debug.LogError("Impossible Scene");
            return;
        }

        LoadScene(targetScene, "Down");
    }
}
