using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class PauseMenuHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas pauseCanvas;
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private Volume volume;
    [SerializeField] private PlayerMovement localPlayer;
    private MenuNetworker ntwk;

    [Header("Menu Pages")]
    [SerializeField] GameObject Default;
    [SerializeField] GameObject Settings;

    [Header("Else")]
    [SerializeField] private string mainMenuName = "";

    private DepthOfField depth;

    [Header("Info")]
    public bool paused;
    private bool settingsOpen;
    public bool setup { get; private set; }

    private void Start()
    {
        setup = false;
    }

    public void Setup()
    {
        if (input == null) { input = GameObject.FindGameObjectWithTag("Input").GetComponent<PlayerInputHandler>(); }
        if (volume == null) { volume = GameObject.FindGameObjectWithTag("Volume").GetComponent<Volume>(); }
        if (depth == null) { volume.profile.TryGet<DepthOfField>(out depth); }
        if (localPlayer == null)
        {
            foreach (PlayerMovement player in FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None))
            {
                if (player.IsOwner)
                {
                    localPlayer = player;
                    break;
                }
            }
        }

        pauseCanvas.enabled = false;
        paused = false;
        settingsOpen = false;
        Settings.SetActive(false);

        setup = true;
    }

    private void Update()
    {
        if (!setup) return;

        if (input.PausePressed && !settingsOpen) { GamePause(); }
        else if (input.PausePressed && settingsOpen) { PauseSettingsToggle(); }
    }

    public void GamePause()
    {
        switch (paused)
        {
            case false:
                pauseCanvas.enabled = true;
                Blur(true);
                CursorLocker.Unlock();

                if (localPlayer != null)
                    localPlayer.IsPaused = true;

                paused = true;
                break;

            case true:
                pauseCanvas.enabled = false;
                Blur(false);
                CursorLocker.Lock();

                if (localPlayer != null)
                    localPlayer.IsPaused = false;

                paused = false;
                break;
        }
    }


    private void Blur(bool state)
    {
        switch (state)
        {
            case false:
                depth.mode.value = DepthOfFieldMode.Off;
                break;

            case true:
                depth.mode.value = DepthOfFieldMode.Gaussian;
                break;
        }
    }

    public void PauseSettingsToggle()
    {
        switch (settingsOpen)
        {
            case false:
                Default.SetActive(false);
                Settings.SetActive(true);
                settingsOpen = true;
                break;

            case true:
                Default.SetActive(true);
                Settings.SetActive(false);
                settingsOpen = false;
                break;
        }
    }

    public void PauseGameQuit()
    {
        Application.Quit();
    }

    public void PauseToMainMenu()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }

        SceneManager.LoadScene(mainMenuName);
    }
}
