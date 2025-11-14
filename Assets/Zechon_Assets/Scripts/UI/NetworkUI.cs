using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode.Transports.UTP;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Net.Security;
using Unity.Services.Relay;
using System.Threading.Tasks;
using Unity.Services.Relay.Models;

public class NetworkUI : MonoBehaviour
{
    [Header("Network Manager")]
    public UnityTransport ntwk;

    [Header("UI References (Assign in Inspector)")]
    public Button hostButton;
    public Button clientButton;
    public Button lanButton;
    public Button onlineButton;
    public TMP_Text statusLabel;
    public TMP_InputField usernameInput;
    public TMP_InputField portInput;
    public TMP_InputField ipInput;

    [Header("Online Stuff")]
    public TMP_Text joinCode;
    public TMP_InputField _joinInput;
    public Button _oHost;
    public Button _oJoin;

    [Header("Spawn Info")]
    public Vector3 spawnPosition;
    public int MaxPlayers = 4;

    [SerializeField] private UnityTransport _transport;

    private async void Awake()
    {
        SetLANButtons(false);
        SetOButtons(false);

        await Authenticate();
    }

    private static async Task Authenticate()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Debug.Log("[Unity Services] Authenticated as: " + AuthenticationService.Instance.PlayerId);
    }

    private void OnEnable()
    {
        if (ipInput != null)
            ipInput.text = GetLocalIPAddress();

        if (portInput != null)
            portInput.text = "7777";
    }


    public void OnHostButtonClickedLAN()
    {
        string hostIp = GetLocalIPAddress();
        ushort port = ushort.TryParse(portInput.text, out ushort parsedPort) ? parsedPort : (ushort)7777;

        ntwk.SetConnectionData(hostIp, port);

        NetworkManager.Singleton.StartHost();

        Debug.Log($"[Host] Hosting on {hostIp}:{port}");

        if (ipInput != null)
            ipInput.text = hostIp;

        SendUsernameToPlayer();

        ModeButtonsToggle(false);
        SetLANButtons(false);
    }

    public void OnClientButtonClickedLAN()
    {
        string ip = string.IsNullOrEmpty(ipInput.text) ? "127.0.0.1" : ipInput.text;
        ushort port = ushort.TryParse(portInput.text, out ushort parsedPort) ? parsedPort : (ushort)7777;

        ntwk.SetConnectionData(ip, port);

        NetworkManager.Singleton.StartClient();

        NetworkManager.Singleton.OnClientConnectedCallback += id =>
        {
            if (id == NetworkManager.Singleton.LocalClientId)
                SendUsernameToPlayer();
        };

        ModeButtonsToggle(false);
        SetLANButtons(false);
    }

    private void SendUsernameToPlayer()
    {
        string username = usernameInput.text;
        usernameInput.text = "";

        var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (player == null)
        {
            Debug.LogWarning("Local player object still not spawned.");
            return;
        }

        var handler = player.GetComponent<UsernameHandler>();
        if (handler != null)
        {
            handler.SetUsername(username);
        }
    }

    private void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (NetworkManager.Singleton == null)
        {
            SetStatusText("NetworkManager not found");
            return;
        }

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            SetStatusText("Not connected");
        }
        else
        {
            UpdateStatusLabels();
        }
    }

    private void SetLANButtons(bool state)
    {
        hostButton.gameObject.SetActive(state);
        clientButton.gameObject.SetActive(state);
        usernameInput.gameObject.SetActive(state);
        portInput.gameObject.SetActive(state);
        ipInput.gameObject.SetActive(state);
    }

    private void SetStatusText(string text)
    {
        statusLabel.text = text;
    }

    private void UpdateStatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost
            ? "Host"
            : NetworkManager.Singleton.IsServer
                ? "Server"
                : "Client";

        string modeText = "Mode: " + mode;

        if (mode == "Host")
        {
            string hostIp = GetLocalIPAddress();
            ushort port = ushort.TryParse(portInput.text, out ushort parsedPort) ? parsedPort : (ushort)7777;
            SetStatusText($"{modeText} | {hostIp} | {port}");
        }

        else
        {
            string ip = string.IsNullOrEmpty(ipInput.text) ? "127.0.0.1" : ipInput.text;
            ushort port = ushort.TryParse(portInput.text, out ushort parsedPort) ? parsedPort : (ushort)7777;
            SetStatusText($"{modeText} | {ip} | {port}");
        }
        
    }
    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += id =>
        {
            Debug.Log($"Client connected: {id}");
        };
        NetworkManager.Singleton.OnClientDisconnectCallback += id =>
        {
            Debug.Log($"Client disconnected: {id}");
        };
    }

    private string GetLocalIPAddress()
    {
        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up)
                continue;

            foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
            {
                if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (!IPAddress.IsLoopback(ip.Address))
                    {
                        return ip.Address.ToString();
                    }
                }
            }
        }

        Debug.LogWarning("No valid LAN IPv4 address found, defaulting to 127.0.0.1");
        return "127.0.0.1";
    }

    public void lanChosen()
    {
        SetLANButtons(true);
        SetOButtons(false);
        ModeButtonsToggle(false);
    }

    public void OnlineChosen()
    {
        SetLANButtons(false);
        SetOButtons(true);
        ModeButtonsToggle(false);
    }

    private void SetOButtons(bool state)
    {
        _oHost.gameObject.SetActive(state);
        _oJoin.gameObject.SetActive(state);
        usernameInput.gameObject.SetActive(state);
        _joinInput.gameObject.SetActive(state);
    }


    private void ModeButtonsToggle(bool state)
    {
        lanButton.gameObject.SetActive(state);
        onlineButton.gameObject.SetActive(state);
    }

    public async void HostGame()
    {
        Allocation a = await RelayService.Instance.CreateAllocationAsync(MaxPlayers);
        joinCode.text = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

        _transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);

        NetworkManager.Singleton.StartHost();

        SendUsernameToPlayer();

        ModeButtonsToggle(false);
        SetLANButtons(false);
        SetOButtons(false);
    }

    public async void JoinGame()
    {
        JoinAllocation a = await RelayService.Instance.JoinAllocationAsync(_joinInput.text);

        _transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);

        NetworkManager.Singleton.StartClient();

        NetworkManager.Singleton.OnClientConnectedCallback += id =>
        {
            if (id == NetworkManager.Singleton.LocalClientId)
                SendUsernameToPlayer();
        };

        ModeButtonsToggle(false);
        SetLANButtons(false);
        SetOButtons(false);
    }
}
