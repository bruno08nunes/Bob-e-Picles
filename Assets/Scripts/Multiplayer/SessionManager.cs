
using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    [SerializeField] private int maxPlayers = 2;
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text joinCodeText;
    [SerializeField] private Transform roomListContent;
    [SerializeField] private RoomListItem roomItemPrefab;

    private ISession currentSession;
    private bool servicesReady;
    private bool busy;
    private bool secondPlayerAdded;
    private bool leavingRoom;

    private const string HostPlayerId = "Player 1";
    private const string SecondPlayerId = "Player 2";

    public ISession CurrentSession => currentSession;

    private async void Start()
    {
        await InitializeServices();
    }

    private async Task InitializeServices()
    {
        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            servicesReady = true;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async void CreateRoom()
    {
        if (!CanStartOperation())
            return;

        busy = true;

        try
        {
            string roomName = roomNameInput.text.Trim();

            if (string.IsNullOrWhiteSpace(roomName))
            {
                SetStatus("Defina um nome para a sala.");
                return;
            }

            SetStatus("Criando sala...");

            var options = new SessionOptions
            {
                Name = roomName,
                MaxPlayers = maxPlayers,
                IsPrivate = false
            }.WithRelayNetwork();

            currentSession = await MultiplayerService.Instance.CreateSessionAsync(options);

            secondPlayerAdded = false;
            leavingRoom = false;

            if (joinCodeText != null)
            {
                joinCodeText.text = "Código: " + currentSession.Code + "\nNome: " + currentSession.Name;
            }

            SetStatus($"Sala criada! Código: {currentSession.Code}");

            Debug.Log($"Sala criada: {currentSession.Name} " + $"| Código: {currentSession.Code}");

            RoomMenuManager.ChangeToRoomSection();

            RoomMenuManager.AddPlayerListItem("Player 1", "Bob");

            SubscribeToSessionEvents();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            SetStatus($"Erro ao criar sala: {e.Message}");
        }
        finally
        {
            busy = false;
            leavingRoom = false;
        }
    }

    public async void JoinRoomByCode()
    {
        if (!CanStartOperation())
            return;

        string code = joinCodeInput.text.Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            SetStatus("Digite um código de convite.");
            return;
        }

        busy = true;

        try
        {
            SetStatus("Entrando na sala...");

            currentSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(code);

            secondPlayerAdded = false;
            leavingRoom = false;

            SubscribeToSessionEvents();

            if (joinCodeText != null)
            {
                joinCodeText.text =
                    "Código: " + currentSession.Code +
                    "\nNome: " + currentSession.Name;
            }

            RoomMenuManager.ChangeToRoomSection();

            RoomMenuManager.AddPlayerListItem(HostPlayerId, "Bob");
            UpdateSecondPlayerItem();

            SetStatus($"Você entrou em: {currentSession.Name}");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            SetStatus($"Não foi possível entrar: {e.Message}");
        }
        finally
        {
            busy = false;
        }
    }

    public async void RefreshRoomList()
    {
        if (!CanStartOperation())
            return;

        busy = true;

        try
        {
            SetStatus("Buscando salas...");

            ClearRoomList();

            var results = await MultiplayerService.Instance.QuerySessionsAsync(new QuerySessionsOptions());

            foreach (var sessionInfo in results.Sessions)
            {
                // A consulta retorna salas publicas que ainda possuem vagas, conforme os filtros do servico.
                RoomListItem item = Instantiate(roomItemPrefab, roomListContent);

                int occupiedPlayers = sessionInfo.MaxPlayers - sessionInfo.AvailableSlots;

                item.Setup(sessionInfo.Name, occupiedPlayers, sessionInfo.MaxPlayers, () => JoinRoomById(sessionInfo.Id));
            }

            SetStatus($"Salas encontradas: {results.Sessions.Count}");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            SetStatus($"Erro ao buscar salas: {e.Message}");
        }
        finally
        {
            busy = false;
        }
    }

    private async void JoinRoomById(string sessionId)
    {
        if (!CanStartOperation())
            return;

        busy = true;

        try
        {
            SetStatus("Entrando na sala...");

            currentSession = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId);

            secondPlayerAdded = false;
            leavingRoom = false;

            SubscribeToSessionEvents();

            if (joinCodeText != null)
            {
                joinCodeText.text =
                    "Código: " + currentSession.Code +
                    "\nNome: " + currentSession.Name;
            }

            RoomMenuManager.ChangeToRoomSection();

            RoomMenuManager.AddPlayerListItem(HostPlayerId, "Bob");

            UpdateSecondPlayerItem();

            SetStatus($"Você entrou em: {currentSession.Name}");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            SetStatus($"Não foi possível entrar: {e.Message}");
        }
        finally
        {
            busy = false;
        }
    }

    private void SubscribeToSessionEvents()
    {
        if (currentSession == null)
            return;

        currentSession.PlayerJoined += OnPlayerJoined;
        currentSession.PlayerHasLeft += OnPlayerHasLeft;
        currentSession.RemovedFromSession += OnRemovedFromSession;
        currentSession.Deleted += OnSessionDeleted;

        UpdateSecondPlayerItem();
    }

    private void UnsubscribeFromSessionEvents(ISession session)
    {
        if (session == null)
            return;

        session.PlayerJoined -= OnPlayerJoined;
        session.PlayerHasLeft -= OnPlayerHasLeft;
        session.RemovedFromSession -= OnRemovedFromSession;
        session.Deleted -= OnSessionDeleted;
    }

    private void OnPlayerJoined(string playerId)
    {
        Debug.Log($"Jogador entrou na sessão: {playerId}");

        UpdateSecondPlayerItem();
    }

    private void OnPlayerHasLeft(string playerId)
    {
        Debug.Log($"Jogador saiu da sessão: {playerId}");

        UpdateSecondPlayerItem();
    }

    private void UpdateSecondPlayerItem()
    {
        if (currentSession == null)
            return;

        bool hasSecondPlayer = currentSession.Players.Count >= 2;

        if (hasSecondPlayer && !secondPlayerAdded)
        {
            RoomMenuManager.AddPlayerListItem(
                SecondPlayerId,
                "Picles");

            secondPlayerAdded = true;

            SetStatus("Segundo Jogador entrou na sala!");
        }
        else if (!hasSecondPlayer && secondPlayerAdded)
        {
            RoomMenuManager.RemovePlayerListItem(SecondPlayerId);

            secondPlayerAdded = false;

            SetStatus("Segundo Jogador saiu da sala.");
        }
    }

    private void OnRemovedFromSession()
    {
        Debug.Log("Você foi removido da sessão.");

        CleanupAfterDisconnection("O host encerrou a sala.");
    }

    private void OnSessionDeleted()
    {
        Debug.Log("A sessão foi excluída.");

        CleanupAfterDisconnection("A sala foi encerrada.");
    }

    private void CleanupAfterDisconnection(string message)
    {
        ISession oldSession = currentSession;

        currentSession = null;
        secondPlayerAdded = false;

        UnsubscribeFromSessionEvents(oldSession);

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        if (joinCodeText != null)
            joinCodeText.text = "";

        RoomMenuManager.RemovePlayerListItem(SecondPlayerId);
        RoomMenuManager.RemovePlayerListItem(HostPlayerId);

        RoomMenuManager.ChangeToJoinRoomSectionStatic();

        SetStatus(message);
    }

    public async void LeaveRoom()
    {
        if (busy || currentSession == null || leavingRoom)
            return;

        busy = true;
        leavingRoom = true;

        try
        {
            SetStatus("Saindo da sala...");

            ISession sessionToLeave = currentSession;
            UnsubscribeFromSessionEvents(sessionToLeave);

            if (sessionToLeave.IsHost)
            {
                await sessionToLeave.AsHost().DeleteAsync();
            }
            else
            {
                await sessionToLeave.LeaveAsync();
            }

            currentSession = null;

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.Shutdown();
            }

            if (joinCodeText != null)
                joinCodeText.text = "";

            if (secondPlayerAdded)
            {
                RoomMenuManager.RemovePlayerListItem(SecondPlayerId);
                secondPlayerAdded = false;
            }

            RoomMenuManager.RemovePlayerListItem(HostPlayerId);

            RoomMenuManager.ChangeToJoinRoomSectionStatic();

            SetStatus("Você saiu da sala.");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            SetStatus($"Erro ao sair: {e.Message}");
        }
        finally
        {
            busy = false;
        }
    }

    private bool CanStartOperation()
    {
        if (!servicesReady)
        {
            SetStatus("Aguarde a inicialização dos serviços.");
            return false;
        }

        if (busy)
            return false;

        if (currentSession != null)
        {
            SetStatus("Você já está em uma sala.");
            return false;
        }

        return true;
    }

    private void ClearRoomList()
    {
        if (roomListContent == null)
            return;

        foreach (Transform child in roomListContent)
            Destroy(child.gameObject);
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;

        Debug.Log(message);
    }
}
