using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using TMPro;
using Unity.Netcode;

public class TestLobby : MonoBehaviour
{

                        public GameObject PlayerPrefab;
    [SerializeField]    private Relay relay;
    [SerializeField]    private GameObject UiManager;
    [SerializeField]    private TMP_Text PlayerName;
                        private Lobby hostLobby;
    [HideInInspector]   public Lobby joinedLobby;
                        private float HeartBeatTimer;
                        private float LobbyUpdateTimer;
    [HideInInspector]   public string playerName;
    [HideInInspector]   public string playerRank;
    [SerializeField]    private TMP_InputField LobbyField;
    [HideInInspector]   public QueryResponse Lobby_List;
                        private GameObject PlayerObject;
    private void Awake()
    {
        playerRank = "Admin";
        Debug.Log(playerName);
        if(PlayerName == null)
        {
            playerName = "Player";
        }
        PlayerName.text = playerName;
    }
    public void refreshPlayerName()
    {
        playerName += " #" + Random.Range(1000, 9999);
        Debug.Log(playerName);
        PlayerName.text = playerName;
    }
    private async void Start()
    {
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
        //Debug.Log(joinedLobby.Name);
    }
    private async void HandleLobbyHeartbeat()
    {
        if(hostLobby != null)
        {
            HeartBeatTimer -= Time.deltaTime;
            if(HeartBeatTimer <= 0f)
            {
                float HeartbeatTimerMax = 15f;
                HeartBeatTimer = HeartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }
    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby != null)
        {
            bool playerStillInLobby = false;
            foreach (Player player in joinedLobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    playerStillInLobby = true;
                }
            }
            LobbyUpdateTimer -= Time.deltaTime;
            if (LobbyUpdateTimer <= 0f)
            {
                float LobbyUpdateTimerMax = 1.2f;
                LobbyUpdateTimer = LobbyUpdateTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                Debug.Log("Lobby Reload!");
                joinedLobby = lobby;
                if (joinedLobby.Data["RelayJoinCode"].Value != "0")
                {
                    Debug.Log("Starting Multiplayer...");
                    UiManager.GetComponent<UiManager>().closeUI();
                    if (!IsLobbyHost())
                    {
                        relay.joinRelay(joinedLobby.Data["RelayJoinCode"].Value);
                    }
                    joinedLobby = null;
                }
                if (joinedLobby.Data["SinglePlayerState"].Value == "1")
                {
                    Debug.Log("Starting Game in Single Player Mode...");
                    UiManager.GetComponent<UiManager>().closeUI();
                    if ((!IsLobbyHost()) && PlayerObject == null)
                    {
                        PlayerObject = Instantiate(PlayerPrefab);
                        PlayerObject.GetComponent<PlayerMovement>().InitializePlayer();
                        PlayerObject.GetComponent<PlayerMovement>().NeedsNetwork = false;
                    }
                    //joinedLobby = null;
                }
            }
            
        }
    }
    public void CreateLobby_P(string LobbyName, int MaxPlayers, string GameMode, bool triggerFunc)
    {
        CreateLobby(LobbyName, MaxPlayers, GameMode, triggerFunc);
    }
    private async void CreateLobby(string LobbyName, int MaxPlayers, string GameMode, bool triggerFunc)
    {
        try
        {
            //string LobbyName = "Test Lobby";
            //int MaxPlayers = 35;
            CreateLobbyOptions createLobbyOptions = new()
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, GameMode, DataObject.IndexOptions.S1)},
                    {"RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, "0")},
                    {"SinglePlayerState", new DataObject(DataObject.VisibilityOptions.Public, "0")} 
                    /* 
                     Definition for "SinglePlayerState"
                     0 = Game not Stated
                     1 = Game begone. Players explore the map in Single player mode, 
                     until everyone joins
                     2 = A Relay join code is created
                     3 = Maybe a state were the relay is deleted but the lobby is kept.
                     Everyone goes back to single player mode
                     4 = The Player gets back to the main lobby menu. the Relay and the Lobby get deleted
                     */
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(LobbyName, MaxPlayers, createLobbyOptions);
            hostLobby = lobby;
            joinedLobby = hostLobby;
            Debug.Log("Created Lobby! Name: " + lobby.Name + " MaxPlayers: " + lobby.MaxPlayers + 
            " Gamemode: " + lobby.Data["GameMode"].Value + " Id: " + lobby.Id + " Lobby Code: " + lobby.LobbyCode);
            if (triggerFunc)
            {
                UiManager.GetComponent<UiManager>().OpenPlayerPanel(lobby.Id, true);
            }
            PrintPlayers(hostLobby);
        } catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new()
            {
                Count = 10,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created),
                }
            };
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            Lobby_List = queryResponse;
            Debug.Log("Lobbies found : " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log("Lobby found: " + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Data["GameMode"].Value);
                PrintPlayers(lobby);
            }           
        }catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }   
    public async void JoinLobby(string lobbyJoinCode)
    {
        Debug.Log("Trying to join Lobby with code: " + LobbyField.text);
        try
        {
            JoinLobbyByIdOptions joinLobbyByIdOptions = new()
            {
                Player = GetPlayer(),
            };
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyJoinCode, joinLobbyByIdOptions);
            joinedLobby = lobby;
            Debug.Log("Succesfully joined Lobby with code: " + LobbyField.text);
            PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void QuickJoinLobby()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();
        }catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)},
                {"PlayerRank", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerRank)},
                {"GameState", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "0")}
            }
        };  
    }
    public void PrintPlayers()
    {
        PrintPlayers(joinedLobby);
    }
    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in Lobby " + lobby.Name + " " + lobby.Data["GameMode"].Value);
        foreach(Player player in lobby.Players)
        {
            Debug.Log(player.Id + " Name: " + player.Data["PlayerName"].Value + " Rank: " + player.Data["PlayerRank"].Value);
        }
    }
    public async void UpdatePlayerName(string NewPlayerName)
    {
        try
        {
            playerName = NewPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
            {
                {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
            }
            });
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            Debug.Log("Left lobby with ID: " + joinedLobby.Id);
            if(joinedLobby.Players.Count <= 1)
            {
                DeleteLobby();
            }
            joinedLobby = null;
        }catch(LobbyServiceException e) 
        {
            Debug.Log(e); 
        }
    }
    public async void KickPlayer(string playerId)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            Debug.Log("Left lobby with ID: " + joinedLobby.Id);
            //joinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void MigrateLobbyHost(string PlayerId)
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = PlayerId,
            });
        }catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void DeleteLobby()
    {
        string lobbyName = joinedLobby.Name;
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
            Debug.Log("Deletet Lobby " + "'" + lobbyName + "'");
        }catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public bool IsLobbyHost()
    {
        if(joinedLobby.HostId == AuthenticationService.Instance.PlayerId)
        {
            return true;
        }
        else
        {
            return false;
        }
        
    }
    public async void StartGame()
    {
        if (IsLobbyHost())
        {
            try
            {
                string relayCode = await relay.CreateRelay();
                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        {"RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode)}
                    }
                });
                Debug.Log(lobby.Data["RelayJoinCode"].Value);
                joinedLobby = lobby;
                UiManager.GetComponent<UiManager>().closeUI();
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
    public async void StartInSinglePlayerMode()
    {
        if (IsLobbyHost())
        {
            try
            {
                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        {"SinglePlayerState", new DataObject(DataObject.VisibilityOptions.Public, "1")}
                    }
                });
                //Debug.Log(lobby.Data["RelayJoinCode"].Value);             
                joinedLobby = lobby;
                UiManager.GetComponent<UiManager>().closeUI();
                if (PlayerObject == null)
                {
                    PlayerObject = Instantiate(PlayerPrefab);
                    PlayerObject.GetComponent<PlayerMovement>().InitializePlayer();
                    PlayerObject.GetComponent<PlayerMovement>().NeedsNetwork = false;
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
    }
