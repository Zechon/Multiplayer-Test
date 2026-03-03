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
    UnityTransport transport;

    [Header("UI References")]
    public TMP_InputField usernameInput;
    public TMP_InputField onlineUsernameInput;

    [Header("Online Stuff")]
    public string joinCode;
    private string username;

    [Header("Debug Stuff")]
    private string networkMode = "";
    public string joinIP = "";
    public string joinPort = "";

    [Header("Spawn Info")]
    public Vector3 spawnPosition;
    public int MaxPlayers = 4;

    private string debugKey;
    private string _cachedLocalIP;

    private void Start()
    {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    }

    public void HostClickedLAN(string portInput)
    {
        string hostIp = GetLocalIPAddress();
        ushort port = ushort.TryParse(portInput, out ushort parsedPort) ? parsedPort : (ushort)7777;

        transport.SetConnectionData(hostIp, port);

        MenuNetworkerCachedUsername.Value = usernameInput.text.ToString();

        NetworkManager.Singleton.StartHost();

        Debug.Log($"[Host] Hosting on {hostIp}:{port}");
        networkMode = "LAN";
        joinIP = hostIp;
        joinPort = port.ToString();

        NetworkSessionManager.Instance.SetSessionData("N/A", hostIp, port.ToString());

        GameDebugStuff();

        NetworkManager.Singleton.SceneManager.LoadScene("MP_VOX_TEST", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void ClientClickedLAN(string ipInput, string portInput)
    {
        string ip = string.IsNullOrEmpty(ipInput) ? "127.0.0.1" : ipInput;
        ushort port = ushort.TryParse(portInput, out ushort parsedPort) ? parsedPort : (ushort)7777;

        transport.SetConnectionData(ip, port);

        NetworkSessionManager.Instance.SetSessionData("N/A", ip, port.ToString());

        MenuNetworkerCachedUsername.Value = usernameInput.text.ToString();

        NetworkManager.Singleton.StartClient();
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

        transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);

        NetworkManager.Singleton.StartHost();

        NetworkManager.Singleton.SceneManager.LoadScene("MP_VOX_TEST", UnityEngine.SceneManagement.LoadSceneMode.Single);

        networkMode = "Online";

        NetworkSessionManager.Instance.SetSessionData(joinCode, "N/A", "N/A");

        GameDebugStuff();
    }

    public async void JoinGame(string joinInput)
    {
        MenuNetworkerCachedUsername.Value = onlineUsernameInput.text.ToString();

        JoinAllocation a = await RelayService.Instance.JoinAllocationAsync(joinInput);

        transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port,
            a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);

        joinCode = joinInput;

        NetworkSessionManager.Instance.SetSessionData(joinCode, "N/A", "N/A");

        NetworkManager.Singleton.StartClient();
    }

    private void GameDebugStuff()
    {
        debugKey = "Host" + AuthenticationService.Instance.PlayerId;

        if (networkMode == "LAN") joinCode = "NULL";
        else if (networkMode == "Online")
        {
            joinIP = "NULL";
            joinPort = "NULL";
        }

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
            $"Network Mode: {networkMode}\n" +
            $"Join Code: {joinCode}\n" +
            $"LAN IP Address: {joinIP}\n" +
            $"LAN Port: {joinPort}",
            0
        ));

        return root;
    }
}
