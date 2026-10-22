
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private Button joinButton;

    public void Setup(
        string roomName,
        int currentPlayers,
        int maxPlayers,
        Action onJoin)
    {
        roomNameText.text = roomName;

        playerCountText.text = $"{currentPlayers}/{maxPlayers} jogadores";

        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(() => onJoin?.Invoke());
    }
}
