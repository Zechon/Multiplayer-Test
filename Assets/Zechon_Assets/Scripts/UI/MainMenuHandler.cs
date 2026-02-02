using System.Collections;
using TMPro;
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
    private bool host = true;

    [Header("Scenes")]
    [SerializeField] private string DevScn1Name;
    [SerializeField] private string DevScn2Name;

    [Header("References")]
    private PlayerInputHandler input;
    private MenuNetworker networker;
    private Animator mainAnim;
    private Animator lanAnim;
    private GameObject MainButtons;
    private GameObject LanButtons;
    #endregion

    private void Start()
    {
        SetupComponents();

        lanOpen = false;
        lanAnim.SetBool("Closed", true);

        settingsOpen = false;
        Settings.SetActive(false);

        MainMenu.SetActive(true);
        mainAnim.SetBool("Closed", false);
    }

    private void Update()
    {
        if (input.PausePressed) CloseSubMenus();
    }

    private void SetupComponents()
    {
        if (input == null) input = GetComponent<PlayerInputHandler>();
        if (networker == null) networker = GetComponent<MenuNetworker>();

        if (mainAnim == null) mainAnim = MainMenu.GetComponent<Animator>();
        if(lanAnim == null) lanAnim = LAN.GetComponent<Animator>();
        if (MainButtons == null) MainButtons = MainMenu.transform.GetChild(0).gameObject;
        if (LanButtons == null) LanButtons = LAN.transform.GetChild(0).gameObject;
    }

    #region Public Functions + Helpers
    public void CloseSubMenus()
    {
        if (lanOpen)
        {
            StartCoroutine(CloseLan());
        }

        if (settingsOpen)
        {
            settingsOpen = false;
            Settings.SetActive(false);
        }
    }

    public void OpenLanMenu()
    {
        StartCoroutine(OpenLan());
    }

    private IEnumerator OpenLan()
    {
        mainAnim.SetBool("Closed", true);

        yield return new WaitForSeconds(1);

        lanAnim.SetBool("Closed", false);
        lanOpen = true;
    }
    private IEnumerator CloseLan()
    {
        lanAnim.SetBool("Closed", true);

        yield return new WaitForSeconds(0.95f);

        mainAnim.SetBool("Closed", false);
    }

    public void HostTransition()
    {
        GameObject StartHost = LanButtons.transform.GetChild(3).gameObject;
        TMP_Text ipText = StartHost.transform.GetChild(0).GetComponent<TMP_Text>();

        ipText.text = networker.GetLocalIPAddress();
        host = true;
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
