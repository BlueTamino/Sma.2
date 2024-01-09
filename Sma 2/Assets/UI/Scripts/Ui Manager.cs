using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour
{
    [Space]
    [Header("---Lobby Main Menu---")]
    [SerializeField]
    private GameObject LobbyMainGUI;
    [SerializeField]
    private GameObject PlayerNamePanel;
    [SerializeField]
    private GameObject LobbyPanelPrefab;
    [SerializeField]
    private GameObject LobbyCreateMenu;
    [SerializeField]
    private GameObject LobbySelectMenu;
    [SerializeField]
    private GameObject LobbyLoadingScreen;
    [Space]
    [Header("---Player List---")]
    [SerializeField]
    private GameObject PlayerListMenu;
    [SerializeField]
    private GameObject PlayerPanelPrefab;
    [SerializeField]
    private GameObject PlayerList;
    [Space]
    [Header("---Other Values---")]
    public int AdminState;
    private GameObject NetworkManager;
    private SelectionMenu CreateMenu_LobbyName;
    private SelectionMenu CreateMenu_MaxPlayers;
    private SelectionMenu CreateMenu_GameMode;
    private readonly List<GameObject> LobbyPanelList;
    private bool LobbyCreateMenuActive;
    private bool canCreateLobby;
    private bool isHost_P;
    private float PlayerRefreshTimer = 2f;
    private float LobbyRefreshTimer = 2f;
    private bool LobbyMainGUIActive = true;
    private bool PlayerMenuActive;
    private bool PlayerInLobby = false;
    private void Awake()
    {
        canCreateLobby = true;
        if (!PlayerPrefs.HasKey("Admin State"))
        {
            PlayerPrefs.SetInt("Admin State", AdminState);
        }
        else
        {
            AdminState = PlayerPrefs.GetInt("Admin State");
        }

        //REMOVE ON RELEASE!!!
        PlayerPrefs.SetInt("Admin State", 1);

        NetworkManager = GameObject.FindGameObjectWithTag("Network and Lobby Manager");
        CreateMenu_LobbyName = LobbyCreateMenu.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<SelectionMenu>();
        CreateMenu_MaxPlayers = LobbyCreateMenu.transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<SelectionMenu>();
        CreateMenu_GameMode = LobbyCreateMenu.transform.GetChild(0).GetChild(1).GetChild(2).GetComponent<SelectionMenu>();
        //PlayerName.text = NetworkManager.GetComponent<TestLobby>().playerName;
    }
    private void Update()
    {
        HandleListRefresh();
    }
    private void HandleListRefresh()
    {
        if (PlayerMenuActive)
        {
            PlayerRefreshTimer -= Time.deltaTime;
            if (PlayerRefreshTimer <= 0f)
            {
                float PlayerRefreshTimerMax = 2f;
                PlayerRefreshTimer = PlayerRefreshTimerMax;
                RefreshPlayerList_P();
            }
        }
        if (LobbyMainGUIActive)
        {
            LobbyRefreshTimer -= Time.deltaTime;
            if (LobbyRefreshTimer <= 0f)
            {
                float LobbyRefreshTimerMax = 3f;
                LobbyRefreshTimer = LobbyRefreshTimerMax;
                Refresh();
            }
        }
    }
    public void ToggleLobbyCreateMenu()
    {
        if (canCreateLobby || LobbyCreateMenuActive || AdminState > 0)
        {
            LobbyCreateMenuActive = !LobbyCreateMenuActive;
            LobbyCreateMenu.SetActive(LobbyCreateMenuActive);
        }
    }
    public void Refresh()
    {
        Refresh_New();
        //StartCoroutine(nameof(RefreshCourotine), 1f);
    }
    public async void Refresh_New()
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
            QueryResponse Lobby_List = queryResponse;
            int I = 0;
            //int X = 0;
            GameObject[] oldPanels = GameObject.FindGameObjectsWithTag("Lobby Element");
            if (oldPanels != null)
            {
                foreach (GameObject go in oldPanels)
                {
                    //if (!(X == 0))
                    //
                    Destroy(go);
                    //}
                    //X++;
                }
            }
            foreach (Lobby lobby in Lobby_List.Results)
            {
                Debug.Log("Lobby found: " + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Data["GameMode"].Value + " " + lobby.Players.Count + " " + I + " " + lobby.Id);
                CreateLobbyPanel(lobby.Name, lobby.Players.Count, lobby.MaxPlayers, lobby.Data["GameMode"].Value, I, lobby.Id);
                I++;
            }
            Debug.Log("Lobbies found : " + queryResponse.Results.Count);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    IEnumerator RefreshCourotine(float time)
    {

        int I = 0;
        //int X = 0;
        GameObject[] oldPanels = GameObject.FindGameObjectsWithTag("Lobby Element");
        if (oldPanels != null)
        {
            foreach (GameObject go in oldPanels)
            {
                //if (!(X == 0))
                //
                Destroy(go);
                //}
                //X++;
            }
        }
        LobbyLoadingScreen.SetActive(true);
        LobbyLoadingScreen.GetComponent<Animator>().SetBool("Is Loading", true);
        NetworkManager.GetComponent<TestLobby>().ListLobbies();
        yield return new WaitForSecondsRealtime(time - 1f);
        LobbyLoadingScreen.GetComponent<Animator>().SetBool("Is Loading", false);
        yield return new WaitForSecondsRealtime(1f);
        LobbyLoadingScreen.SetActive(false);
        if (NetworkManager.GetComponent<TestLobby>().Lobby_List.Results != null)
        {
            foreach (Lobby lobby in NetworkManager.GetComponent<TestLobby>().Lobby_List.Results)
            {
                Debug.Log("Lobby found: " + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Data["GameMode"].Value + " " + lobby.Players.Count + " " + I + " " + lobby.Id);
                CreateLobbyPanel(lobby.Name, lobby.Players.Count, lobby.MaxPlayers, lobby.Data["GameMode"].Value, I, lobby.Id);
                I++;
            }
        }
    }
    private void CreateLobbyPanel(string LobbyName, int currentPlayers, int MaxPlayers, string GameMode, int PanelIDX, string lobbyJoinCode)
    {
        GameObject LobbyPanel = Instantiate(LobbyPanelPrefab);
        LobbyPanel.GetComponent<RectTransform>().SetParent(LobbySelectMenu.transform, false);
        //LobbyPanel.GetComponent<RectTransform>().localPosition = new Vector3(0, (LobbySelectMenu.GetComponent<RectTransform>().rect.height / 2) - (PanelIDX * 46), 0);
        LobbyPanel.transform.GetChild(0).GetComponent<TMP_Text>().text = LobbyName;
        LobbyPanel.transform.GetChild(1).GetComponent<TMP_Text>().text = currentPlayers.ToString() + "/" + MaxPlayers.ToString();
        LobbyPanel.transform.GetChild(2).GetComponent<TMP_Text>().text = GameMode;
        LobbyPanel.GetComponent<Button>().onClick.AddListener(
        delegate
        {
            OpenPlayerPanel(lobbyJoinCode, false);
        });
        //LobbyPanelList.Add(LobbyPanel);
    }
    public void OpenPlayerPanel(string lobbyJoinCode, bool IsHost)
    {
        if (!IsHost)
        {
            NetworkManager.GetComponent<TestLobby>().JoinLobby(lobbyJoinCode);
            PlayerListMenu.transform.GetChild(2).GetChild(0).gameObject.SetActive(false);
        }
        PlayerMenuActive = true;
        LobbyMainGUI.SetActive(false);
        LobbyMainGUIActive = false;
        PlayerListMenu.SetActive(true);
        RefreshPlayerList(IsHost);
    }
    public void RefreshPlayerList_P()
    {
        RefreshPlayerList(isHost_P);
    }
    private void RefreshPlayerList(bool isHost)
    {
        isHost_P = isHost;
        GameObject[] oldPanels = GameObject.FindGameObjectsWithTag("Player Element");
        if (oldPanels != null)
        {
            foreach (GameObject go in oldPanels)
            {
                //if (!(X == 0))
                //
                Destroy(go);
                //}
                //X++;
            }
        }
        PlayerInLobby = false;
        foreach (Player player in NetworkManager.GetComponent<TestLobby>().joinedLobby.Players)
        {
            CreatePlayerPanel(player.Data["PlayerName"].Value, player.Id, isHost);
            if ((!PlayerInLobby) && player.Data["PlayerName"].Value == NetworkManager.GetComponent<TestLobby>().playerName)
            {
                PlayerInLobby = true;
            }
            Debug.Log(PlayerInLobby);
        }
        if (!PlayerInLobby)
        {
            Debug.Log("Player Kicked");
            PlayerMenuActive = false;
            PlayerListMenu.SetActive(false);
            LobbyMainGUI.SetActive(true);
        }
    }
    private void CreatePlayerPanel(string PlayerName, string PlayerId, bool isLobbyHost)
    {
        GameObject PlayerPanel = Instantiate(PlayerPanelPrefab, PlayerList.transform);
        PlayerPanel.GetComponentInChildren<TMP_Text>().text = PlayerName;
        //PlayerList.GetComponent<RectTransform>().SetParent(PlayerList.transform, false);
        //Debug.Log(isLobbyHost);
        if (NetworkManager.GetComponent<TestLobby>().IsLobbyHost() && PlayerName != NetworkManager.GetComponent<TestLobby>().playerName)
        {
            PlayerPanel.transform.GetChild(1).gameObject.SetActive(true);
            PlayerPanel.transform.GetChild(1).gameObject.GetComponent<Button>().onClick.AddListener(
            delegate
            {
                NetworkManager.GetComponent<TestLobby>().KickPlayer(PlayerId);
            });

            PlayerPanel.transform.GetChild(2).gameObject.SetActive(true);
            PlayerPanel.transform.GetChild(2).gameObject.GetComponent<Button>().onClick.AddListener(
            delegate
            {
                NetworkManager.GetComponent<TestLobby>().MigrateLobbyHost(PlayerId);
            });
        }
        else
        {
            PlayerPanel.transform.GetChild(1).gameObject.SetActive(false);
            PlayerPanel.transform.GetChild(2).gameObject.SetActive(false);
        }
    }
    private void DebugNumber(int number)
    {
        Debug.Log(number);
    }
    public void CreateLobby()
    {
        if
        (
            CreateMenu_LobbyName.resultString == null || CreateMenu_MaxPlayers.resultInt <= 0 || CreateMenu_GameMode.resultString == null
        )
        {
            Debug.LogError("Could not create a lobby, because value is missing! " + CreateMenu_LobbyName.resultString + " "
            + CreateMenu_MaxPlayers.resultInt + " " + CreateMenu_GameMode.resultString);
            return;
        }
        Debug.Log("Creating Lobby with parameters: " + CreateMenu_LobbyName.resultString + " " + CreateMenu_MaxPlayers.resultInt + " " + CreateMenu_GameMode.resultString);
        NetworkManager.GetComponent<TestLobby>().CreateLobby_P
        (
            CreateMenu_LobbyName.resultString, CreateMenu_MaxPlayers.resultInt, CreateMenu_GameMode.resultString, true
        );
        LobbyCreateMenu.SetActive(false);
        canCreateLobby = false;
        //bool search = true;
        /*while (search)
        {
            if(NetworkManager.GetComponent<TestLobby>().joinedLobby.Id != null)
            {
                search = false;
                OpenPlayerPanel(NetworkManager.GetComponent<TestLobby>().joinedLobby.Id, true);
                Debug.Log(NetworkManager.GetComponent<TestLobby>().joinedLobby.Id);
                Debug.Log("Test");
            }
        }*/
        //StartCoroutine(nameof(RefreshCourotine), 4f);

    }
    public void EnterMainLobbyMenu()
    {
        NetworkManager.GetComponent<TestLobby>().playerName = PlayerNamePanel.transform.GetChild(0).GetChild(1).GetComponentInChildren<TMP_InputField>().text;
        PlayerNamePanel.SetActive(false);
        NetworkManager.GetComponent<TestLobby>().refreshPlayerName();
    }
    public void LeaveLobby()
    {
        PlayerMenuActive = false;
        NetworkManager.GetComponent<TestLobby>().LeaveLobby();
        PlayerListMenu.SetActive(false);
        LobbyMainGUI.SetActive(true);
        Refresh();
    }
    public void closeUI()
    {
       gameObject.transform.GetChild(3).gameObject.SetActive(false);
       PlayerMenuActive = false;
    }
    public void StartInSinglePlayerMode()
    {
        NetworkManager.GetComponent<TestLobby>().StartInSinglePlayerMode();
    }
}
