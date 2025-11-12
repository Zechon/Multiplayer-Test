using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode.Transports.UTP;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

public class NetworkUI : MonoBehaviour
{
    [Header("Network Manager")]
    public UnityTransport ntwk;

    [Header("UI References (Assign in Inspector)")]
    public Button hostButton;
    public Button clientButton;
    public TMP_Text statusLabel;
    public TMP_InputField usernameInput;
    public TMP_InputField portInput;
    public TMP_InputField ipInput;

    [Header("Spawn Info")]
    public Vector3 spawnPosition;

    private void OnEnable()
    {
        hostButton.onClick.AddListener(OnHostButtonClicked);
        clientButton.onClick.AddListener(OnClientButtonClicked);

        if (ipInput != null)
            ipInput.text = GetLocalIPAddress();

        if (portInput != null)
            portInput.text = "7777";
    }

    private void OnDisable()
    {
        hostButton.onClick.RemoveListener(OnHostButtonClicked);
        clientButton.onClick.RemoveListener(OnClientButtonClicked);
    }

    private void OnHostButtonClicked()
    {
        string hostIp = GetLocalIPAddress();
        ushort port = ushort.TryParse(portInput.text, out ushort parsedPort) ? parsedPort : (ushort)7777;

        ntwk.SetConnectionData(hostIp, port);

        NetworkManager.Singleton.StartHost();


        Debug.Log($"[Host] Hosting on {hostIp}:{port}");

        if (ipInput != null)
            ipInput.text = hostIp;

        SendUsernameToPlayer();
    }

    private void OnClientButtonClicked()
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
            SetStartButtons(false);
            SetStatusText("NetworkManager not found");
            return;
        }

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            SetStartButtons(true);
            SetStatusText("Not connected");
        }
        else
        {
            SetStartButtons(false);
            UpdateStatusLabels();
        }
    }

    private void SetStartButtons(bool state)
    {
        hostButton.gameObject.SetActive(state);
        clientButton.gameObject.SetActive(state);
        usernameInput.gameObject.SetActive(state);
        portInput.gameObject.SetActive(state);
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

        SetStatusText($"{modeText}");
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
}
