using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [SerializeField] float volume = 1.0f;
    [SerializeField] float musicVolume = 1.0f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        volume = PlayerPrefs.GetFloat("Volume", 1);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1);
    }

    public static void SaveVolume(float newVolume)
    {
        PlayerPrefs.SetFloat("Volume", newVolume);
        PlayerPrefs.Save();
        Instance.volume = newVolume;
    }

    public static void SaveMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
        Instance.musicVolume = volume;
    }

    public static float GetVolume()
    {
        return Instance.volume;
    }

    public static float GetMusicVolume()
    {
        return Instance.musicVolume;
    }
}