using System.Collections.Generic;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;

public class RoomMenuManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private GameObject createRoomSection;
    [SerializeField] private GameObject joinRoomSection;
    [SerializeField] private GameObject roomListSection;
    [SerializeField] private GameObject roomSection;

    static RoomMenuManager Instance;

    [SerializeField] PlayerListItem playerListItemPrefab;
    [SerializeField] Transform playerList;
    List<PlayerListItem> playerListItems = new();

    void Start()
    {
        if (Instance != null)
        {
            return;
        }
        Instance = this;
    }

    public void ChangeToCreateRoomSection()
    {
        joinRoomSection.SetActive(false);
        roomListSection.SetActive(false);
        roomSection.SetActive(false);

        createRoomSection.SetActive(true);

        title.text = "Criar Sala";
    }

    public void ChangeToJoinRoomSection()
    {
        createRoomSection.SetActive(false);
        roomSection.SetActive(false);

        joinRoomSection.SetActive(true);
        roomListSection.SetActive(true);

        title.text = "Salas";
    }

    public static void ChangeToJoinRoomSectionStatic()
    {
        Instance.createRoomSection.SetActive(false);
        Instance.roomSection.SetActive(false);

        Instance.joinRoomSection.SetActive(true);
        Instance.roomListSection.SetActive(true);

        Instance.title.text = "Salas";
    }

    public static void ChangeToRoomSection()
    {
        Instance.createRoomSection.SetActive(false);
        Instance.joinRoomSection.SetActive(false);
        Instance.roomListSection.SetActive(false);

        Instance.roomSection.SetActive(true);
    }

    public static void AddPlayerListItem(string username, string character)
    {
        PlayerListItem item = Instantiate(Instance.playerListItemPrefab, Instance.playerList);

        item.Setup(username, character);

        Instance.playerListItems.Add(item);
    }

    public static void RemovePlayerListItem(string username)
    {
        PlayerListItem item = Instance.playerListItems.Find(
            item => item.GetUsername() == username
        );

        if (item == null)
            return;

        Instance.playerListItems.Remove(item);

        Destroy(item.gameObject);
    }

    public static void ChangePlayerCharacter()
    {
        PlayerListItem player1 = Instance.playerListItems[0];
        PlayerListItem player2 = Instance.playerListItems[1];

        if (player1 == null || player2 == null)
        {
            return;
        }

        string player1Character = player1.GetCharacter();
        string player2Character = player2.GetCharacter();

        player1.SetCharacter(player2Character);
        player2.SetCharacter(player1Character);
    }
}
