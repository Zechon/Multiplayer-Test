using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerLocalInteractionsSetup : MonoBehaviour
{
    [Header("Objects / Refs")]
    [SerializeField] private PlayerCamera plyrCamRef;

    private void OnEnable()
    {
        if (!GetComponent<NetworkObject>().IsOwner)
            return;

        if (PauseMenuHandler.Instance != null)
        {
            SetupPause(PauseMenuHandler.Instance);
        }
        else
        {
            PauseMenuHandler.OnPauseMenuReady += SetupPause;
        }
    }

    private void OnDisable()
    {
        PauseMenuHandler.OnPauseMenuReady -= SetupPause;
    }

    private void SetupPause(PauseMenuHandler handler)
    {
        plyrCamRef.PauseHSetup(handler);
        GetComponent<PlayerDebugger>().pauseHSetup(handler);
    }
}
