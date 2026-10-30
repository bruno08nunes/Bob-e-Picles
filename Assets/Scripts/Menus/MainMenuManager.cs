using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartGameLocal()
    {
        Debug.Log("Not Created");

        return;
    }

    public void StartGameOnline()
    {
        SceneManager.LoadScene("RoomMenu");
    }

    public void Settings()
    {
        Debug.Log("Not created");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
