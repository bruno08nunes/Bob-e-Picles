using TMPro;
using UnityEngine;

public class PlayerListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private TMP_Text characterText;

    public void Setup(string username, string character)
    {
        usernameText.text = username;

        characterText.text = character;
    }

    public string GetUsername()
    {
        return usernameText.text;
    }

    public string GetCharacter()
    {
        return characterText.text;
    }

    public void SetCharacter(string character)
    {
        characterText.text = character;
    }
}
