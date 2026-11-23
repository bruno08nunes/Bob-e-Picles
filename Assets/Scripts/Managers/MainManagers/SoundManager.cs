using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        musicSource.volume = SettingsManager.GetMusicVolume();
        sfxSource.volume = SettingsManager.GetVolume();
    }

    public void ChangeMusic(AudioClip clip)
    {
        if (clip == null)
            return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (musicSource.clip != null)
            musicSource.UnPause();
    }

    public void PlaySFX(SoundEffectData soundEffect)
    {
        sfxSource.pitch = soundEffect.pitch;
        sfxSource.PlayOneShot(soundEffect.clip, soundEffect.volume * SettingsManager.GetVolume());
    }
}
