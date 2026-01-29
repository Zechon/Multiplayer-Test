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

    [Header("Menu Pages")]
    [SerializeField] GameObject Default;
    [SerializeField] GameObject Settings;

    [Header("Else")]
    [SerializeField] private string mainMenuName = "";

    private DepthOfField depth;

    [Header("Info")]
    public bool paused;
    private bool settingsOpen;

    private void Start()
    {
        if (input == null) { input = GameObject.FindGameObjectWithTag("Input").GetComponent<PlayerInputHandler>(); }
        if (volume == null) { volume = GameObject.FindGameObjectWithTag("Volume").GetComponent<Volume>(); }
        if (depth == null) { volume.profile.TryGet<DepthOfField>(out depth); }

        pauseCanvas.enabled = false;
        paused = false;
        settingsOpen = false;
        Settings.SetActive(false);
    }

    private void Update()
    {
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
                paused = true;
                break;

            case true:
                pauseCanvas.enabled = false;
                Blur(false);
                CursorLocker.Lock();
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
        SceneManager.LoadScene(mainMenuName);
    }
}
