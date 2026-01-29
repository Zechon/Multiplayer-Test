using UnityEngine;

public class MainMenuHandler : MonoBehaviour
{
    #region Variables
    [Header("Menu Pages")]
    [SerializeField] GameObject MainMenu;
    [SerializeField] GameObject LAN;
    [SerializeField] GameObject Settings;

    [Header("Bools")]
    private bool settingsOpen = true;
    private bool lanOpen = true;

    [Header("Scenes")]
    [SerializeField] private string DevScn1Name;
    [SerializeField] private string DevScn2Name;

    [Header("References")]
    private PlayerInputHandler input;
    #endregion

    private void Start()
    {
        if (input == null) input = GetComponent<PlayerInputHandler>();

        CloseSubMenus();
    }

    private void Update()
    {
        if (input.PausePressed) CloseSubMenus();
    }

    #region Public Functions
    public void CloseSubMenus()
    {
        if (lanOpen)
        {
            lanOpen = false;
            LAN.SetActive(false);
        }

        if (settingsOpen)
        {
            settingsOpen = false;
            Settings.SetActive(false);
        }

        MainMenu.SetActive(true);
    }
    #endregion
}
