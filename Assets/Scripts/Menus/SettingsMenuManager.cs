using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsMenuManager : MonoBehaviour
{
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider volumeSlider;

    private void Start()
    {
        float music = SettingsManager.GetMusicVolume();
        float volume = SettingsManager.GetVolume();

        musicSlider.value = music;
        volumeSlider.value = volume;
    }

    public void SetMusic(float music)
    {
        SettingsManager.SaveMusicVolume(music);
    }

    public void SetVolume(float volume)
    {
        SettingsManager.SaveVolume(volume);
    }

    public void GoBackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}