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
    private bool host = false;
    private bool join = false;

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

    #region Default Stuff
    private void Start()
    {
        SetupComponents();

        lanOpen = false;
        lanAnim.SetBool("Closed", true);

        settingsOpen = false;
        Settings.SetActive(false);

        MainMenu.SetActive(true);
        mainAnim.SetBool("Closed", false);

        host = false;
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
    }

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
    #endregion

    #region LAN
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

        yield return new WaitForSeconds(1);

        mainAnim.SetBool("Closed", false);
        lanAnim.SetBool("OpenH", false);
        lanAnim.SetBool("OpenJ", false);
    }

    public void LanHostPromptActivation()
    {
        if (!host) StartCoroutine(LanHostPromptOpen());
        else StartCoroutine(LanHostPromptClose());
    }

    private IEnumerator LanHostPromptOpen()
    {
        GameObject PopupHolder = LAN.transform.GetChild(1).gameObject;
        GameObject Popup = PopupHolder.transform.GetChild(0).gameObject;
        TMP_Text ipText = Popup.transform.GetChild(3).GetComponent<TMP_Text>();
        //ipText.text = networker.GetLocalIPAddress();
        ipText.text = "Your IP";

        lanAnim.SetBool("OpenH", true);
        yield return new WaitForSeconds(0.5f);

        host = true;
        join = false;

        lanAnim.SetBool("OpenJ", false);
    }

    private IEnumerator LanHostPromptClose()
    {
        lanAnim.SetBool("OpenH", false);
        yield return new WaitForSeconds(0.5f);

        host = false;
    }

    public void LanJoinPromptActivation()
    {
        if (!join) StartCoroutine(LanJoinPromptOpen());
        else StartCoroutine(LanJoinPromptClose());
    }

    private IEnumerator LanJoinPromptOpen()
    {
        GameObject PopupHolder = LAN.transform.GetChild(1).gameObject;
        GameObject Popup = PopupHolder.transform.GetChild(1).gameObject;

        lanAnim.SetBool("OpenJ", true);
        yield return new WaitForSeconds(0.5f);

        join = true;
        host = false;

        lanAnim.SetBool("OpenH", false);
    }

    private IEnumerator LanJoinPromptClose()
    {
        lanAnim.SetBool("OpenJ", false);
        yield return new WaitForSeconds(0.5f);

        join = false;
    }

    public void LanHost()
    {
        GameObject PopupHolder = LAN.transform.GetChild(1).gameObject;
        GameObject Popup = PopupHolder.transform.GetChild(0).gameObject;
        TMP_InputField input = Popup.transform.GetChild(5).GetComponent<TMP_InputField>();

        networker.HostClickedLAN(input.text);
    }

    public void LanJoin()
    {
        GameObject PopupHolder = LAN.transform.GetChild(1).gameObject;
        GameObject Popup = PopupHolder.transform.GetChild(1).gameObject;
        TMP_InputField ipInput = Popup.transform.GetChild(3).GetComponent<TMP_InputField>();
        TMP_InputField portInput = Popup.transform.GetChild(5).GetComponent<TMP_InputField>();

        networker.ClientClickedLAN(ipInput.text, portInput.text);
    }
    #endregion

    #region Online
    public void OpenOnlineMenu()
    {
        //Coming soon
    }
    #endregion

    #region Settings + Quit
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
