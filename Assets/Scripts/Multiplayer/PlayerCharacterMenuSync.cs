using Unity.Netcode;
using UnityEngine;

public class PlayerCharacterMenuSync : NetworkBehaviour
{
    public static PlayerCharacterMenuSync Instance { get; private set; }

    public NetworkVariable<char> Player1Character = new('B', NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<char> Player2Character = new('P', NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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

    private void Start()
    {
        Player1Character.OnValueChanged += OnPlayer1Changed;
    }

    public void ChangePlayerCharacter()
    {
        if (!IsServer)
            return;

        char temp = Player1Character.Value;
        Player1Character.Value = Player2Character.Value;
        Player2Character.Value = temp;
    }

    public static void ResetCharacters()
    {
        Debug.Log("Reset");
        Instance.Player1Character.Value = 'B';
        Instance.Player2Character.Value = 'P';
    }

    public static void UpdateInterface()
    {
        RoomMenuManager.ChangePlayerCharacter(Instance.Player1Character.Value);
    }

    private void OnPlayer1Changed(char prev, char next)
    {
        Debug.Log("Changed");
        RoomMenuManager.ChangePlayerCharacter(Player1Character.Value);
    }
}