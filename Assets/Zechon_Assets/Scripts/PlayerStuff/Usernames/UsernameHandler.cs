using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Collections;

public class UsernameHandler : NetworkBehaviour
{
    [Header("UI Reference")]
    public TMP_Text usernameText;

    private NetworkVariable<FixedString128Bytes> NetworkUsername
        = new NetworkVariable<FixedString128Bytes>(
            writePerm: NetworkVariableWritePermission.Server,
            readPerm: NetworkVariableReadPermission.Everyone
        );

    public void RequestSetUsername(string username)
    {
        if (IsOwner)
            SubmitUsernameServerRpc(username);
    }

    [ServerRpc]
    private void SubmitUsernameServerRpc(string username, ServerRpcParams rpc = default)
    {
        NetworkUsername.Value = username;
    }

    private void Start()
    {
        NetworkUsername.OnValueChanged += (oldValue, newValue) =>
        {
            if (usernameText != null)
                usernameText.text = newValue.ToString();
        };

        usernameText.text = NetworkUsername.Value.ToString();
    }
}
