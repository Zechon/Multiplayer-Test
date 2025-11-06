using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Collections;

public class UsernameHandler : NetworkBehaviour
{
    [Header("UI Reference")]
    public TMP_Text usernameText;

    private NetworkVariable<FixedString128Bytes> NetworkUsername = new NetworkVariable<FixedString128Bytes>(
        writePerm: NetworkVariableWritePermission.Owner,
        readPerm: NetworkVariableReadPermission.Everyone
    );

    public void SetUsername(string username)
    {
        if (!IsOwner) return;
        NetworkUsername.Value = new FixedString128Bytes(username);
    }

    private void Start()
    {
        NetworkUsername.OnValueChanged += (oldValue, newValue) =>
        {
            usernameText.text = newValue.ToString();
        };

        usernameText.text = NetworkUsername.Value.ToString();
    }

}
