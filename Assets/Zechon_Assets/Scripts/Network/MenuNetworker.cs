using Unity.Netcode;
using UnityEngine;
using TMPro;
using Unity.Netcode.Transports.UTP;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using System.Threading.Tasks;
using Unity.Services.Relay.Models;

public class MenuNetworker : MonoBehaviour
{
    [Header("Network Manager")]
    public UnityTransport ntwk;

    [Header("UI References (Assign in Inspector)")]
    public TMP_InputField usernameInput;

    [Header("Online Stuff")]
    public TMP_Text joinCode;

    [Header("Spawn Info")]
    public Vector3 spawnPosition;
    public int MaxPlayers = 4;

    [SerializeField] private UnityTransport _transport;

    private async void Awake()
    {
        await Authenticate();
    }

    private static async Task Authenticate()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Debug.Log("[Unity Services] Authenticated as: " + AuthenticationService.Instance.PlayerId);
    }

    public void HostClickedLAN(string portInput)
    {
        string hostIp = GetLocalIPAddress();
        ushort port = ushort.TryParse(portInput, out ushort parsedPort) ? parsedPort : (ushort)7777;

        ntwk.SetConnectionData(hostIp, port);

        NetworkManager.Singleton.StartHost();

        Debug.Log($"[Host] Hosting on {hostIp}:{port}");

        NetworkManager.Singleton.SceneManager.LoadScene("MP_VOX_TEST", UnityEngine.SceneManagement.LoadSceneMode.Single);

        var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        playerObject.transform.position = spawnPosition;

        string username = usernameInput.text;
        SendUsernameToPlayer(username);
    }

    public void ClientClickedLAN(string ipInput, string portInput)
    {
        string ip = string.IsNullOrEmpty(ipInput) ? "127.0.0.1" : ipInput;
        ushort port = ushort.TryParse(portInput, out ushort parsedPort) ? parsedPort : (ushort)7777;

        ntwk.SetConnectionData(ip, port);

        NetworkManager.Singleton.StartClient();

        NetworkManager.Singleton.OnClientConnectedCallback += id =>
        {
            if (id == NetworkManager.Singleton.LocalClientId)
            {
                var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                playerObject.transform.position = new Vector3(0, 30, 0);
                string username = usernameInput.text;
                SendUsernameToPlayer(username);
            }
        };
    }
    private void SendUsernameToPlayer(string username)
    {
        var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (player == null)
        {
            Debug.LogWarning("Local player object still not spawned.");
            return;
        }

        var handler = player.GetComponent<UsernameHandler>();
        if (handler != null)
        {
            handler.RequestSetUsername(username);
        }
    }
    public string GetLocalIPAddress()
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

    public async void HostGame()
    {
        Allocation a = await RelayService.Instance.CreateAllocationAsync(MaxPlayers);
        joinCode.text = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

        _transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);

        NetworkManager.Singleton.StartHost();

        string username = usernameInput.text;
        SendUsernameToPlayer(username);
    }

    public async void JoinGame(string joinInput)
    {
        JoinAllocation a = await RelayService.Instance.JoinAllocationAsync(joinInput);

        _transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port,
            a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        NetworkManager.Singleton.StartClient();
    }

    private void OnClientConnected(ulong id)
    {
        if (id == NetworkManager.Singleton.LocalClientId)
        {
            string username = usernameInput.text;
            SendUsernameToPlayer(username);
        }
    }
}
