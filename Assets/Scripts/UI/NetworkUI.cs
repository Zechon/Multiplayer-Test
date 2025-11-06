using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkUI : MonoBehaviour
{
    [Header("UI References (Assign in Inspector)")]
    public Button hostButton;
    public Button clientButton;
    public Button serverButton;
    public TMP_Text statusLabel;

    [Header("Spawn Info")]
    public Vector3 spawnPosition;

    private void OnEnable()
    {
        hostButton.onClick.AddListener(OnHostButtonClicked);
        clientButton.onClick.AddListener(OnClientButtonClicked);
        serverButton.onClick.AddListener(OnServerButtonClicked);
    }

    private void OnDisable()
    {
        hostButton.onClick.RemoveListener(OnHostButtonClicked);
        clientButton.onClick.RemoveListener(OnClientButtonClicked);
        serverButton.onClick.RemoveListener(OnServerButtonClicked);
    }

    private void OnHostButtonClicked() => NetworkManager.Singleton.StartHost();

    private void OnClientButtonClicked() => NetworkManager.Singleton.StartClient();

    private void OnServerButtonClicked() => NetworkManager.Singleton.StartServer();

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
        serverButton.gameObject.SetActive(state);
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
}
