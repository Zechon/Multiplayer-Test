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

    [Header("UI References")]
    public TMP_InputField usernameInput;
    public TMP_InputField onlineUsernameInput;

    [Header("Online Stuff")]
    public string joinCode;
    private string username;

    [Header("Spawn Info")]
    public Vector3 spawnPosition;
    public int MaxPlayers = 4;

    [SerializeField] private UnityTransport _transport;
    private string debugKey;

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

        MenuNetworkerCachedUsername.Value = usernameInput.text.ToString();

        NetworkManager.Singleton.StartHost();

        Debug.Log($"[Host] Hosting on {hostIp}:{port}");

        NetworkManager.Singleton.SceneManager.LoadScene("MP_VOX_TEST", UnityEngine.SceneManagement.LoadSceneMode.Single);

        var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        playerObject.transform.position = spawnPosition;
    }

    public void ClientClickedLAN(string ipInput, string portInput)
    {
        string ip = string.IsNullOrEmpty(ipInput) ? "127.0.0.1" : ipInput;
        ushort port = ushort.TryParse(portInput, out ushort parsedPort) ? parsedPort : (ushort)7777;

        ntwk.SetConnectionData(ip, port);

        MenuNetworkerCachedUsername.Value = usernameInput.text.ToString();

        NetworkManager.Singleton.StartClient();

        NetworkManager.Singleton.OnClientConnectedCallback += id =>
        {
            if (id == NetworkManager.Singleton.LocalClientId)
            {
                var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                playerObject.transform.position = new Vector3(0, 30, 0);
            }
        };
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

    public async void HostGame(int playerCountMax)
    {
        MenuNetworkerCachedUsername.Value = onlineUsernameInput.text.ToString();

        Allocation a = await RelayService.Instance.CreateAllocationAsync(MaxPlayers);
        joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);
        Debug.Log(joinCode);

        _transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);

        NetworkManager.Singleton.StartHost();

        NetworkManager.Singleton.SceneManager.LoadScene("MP_VOX_TEST", UnityEngine.SceneManagement.LoadSceneMode.Single);

        var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        playerObject.transform.position = spawnPosition;

        GameDebugStuff();
    }

    public async void JoinGame(string joinInput)
    {
        MenuNetworkerCachedUsername.Value = onlineUsernameInput.text.ToString();

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
            var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            playerObject.transform.position = new Vector3(0, 30, 0);
        }
    }

    private void GameDebugStuff()
    {
        debugKey = "Host" + AuthenticationService.Instance.PlayerId;

        GameDebugRegistry.Register(debugKey, BuildSection);
    }

    private DebugSection BuildSection()
    {
        DebugSection root = new DebugSection(
            debugKey,
            $"Host Client: {onlineUsernameInput.text.ToString()} ({AuthenticationService.Instance.PlayerId})",
            "",
            0
        );

        root.Children.Add(new DebugSection(
            debugKey + "_details",
            "Details",
            $"Join Code: {joinCode}",
            0
        ));

        return root;
    }
}
