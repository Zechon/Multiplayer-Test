using System.Collections;
using System.Security.Cryptography.X509Certificates;
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
    private MenuNetworker networker;
    private Animator mainAnim;
    #endregion

    private void Start()
    {
        SetupComponents();

        CloseSubMenus();
    }

    private void Update()
    {
        if (input.PausePressed) CloseSubMenus();
    }

    private void SetupComponents()
    {
        if (input == null) input = GetComponent<PlayerInputHandler>();
        //if (networker == null) networker = GetComponent<MenuNetworker>();

        if (mainAnim == null) mainAnim = MainMenu.GetComponent<Animator>();
    }

    #region Public Functions + Helpers
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
    public void OpenLanMenu()
    {
        StartCoroutine(OpenLan());
    }

    private IEnumerator OpenLan()
    {
        LAN.SetActive(true);
        lanOpen = true;

        mainAnim.SetBool("Closed", true);
        //AnimationClip clip = mainAnim.GetCurrentAnimatorClipInfo;

        yield return new WaitForSeconds(2);
        MainMenu.SetActive(false);
    }

    public void OpenOnlineMenu()
    {
        //Coming soon
    }

    public void OpenSettings()
    {
        settingsOpen = true;
        Settings.SetActive(true);

        MainMenu.SetActive(false);
    }

    public void MainMenuQuit()
    {
        Application.Quit();
    }
    #endregion
}
