using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Collections;

public class UsernameHandler : NetworkBehaviour
{
    [Header("UI Reference")]
    public TMP_Text usernameText;

    public NetworkVariable<FixedString64Bytes> Username =
        new(writePerm: NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        // Listen for changes
        Username.OnValueChanged += OnUsernameChanged;

        // Update immediately (important for host & late join)
        UpdateUsernameText(Username.Value);

        if (IsOwner)
        {
            string pendingName = MenuNetworkerCachedUsername.Value;
            if (!string.IsNullOrEmpty(pendingName))
            {
                RequestSetUsername(pendingName);
            }
        }
    }

    private void OnUsernameChanged(FixedString64Bytes oldValue, FixedString64Bytes newValue)
    {
        UpdateUsernameText(newValue);
    }

    private void UpdateUsernameText(FixedString64Bytes value)
    {
        if (usernameText != null)
            usernameText.text = value.ToString();
    }

    public void RequestSetUsername(string name)
    {
        if (IsOwner)
            SetUsernameServerRpc(name);
    }

    [ServerRpc]
    private void SetUsernameServerRpc(string name)
    {
        Debug.Log("Server received username: " + name);
        Username.Value = new FixedString64Bytes(name);
    }
}

public static class MenuNetworkerCachedUsername
{
    public static string Value;
}
