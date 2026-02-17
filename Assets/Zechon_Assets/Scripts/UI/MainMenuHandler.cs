using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class MainMenuHandler : MonoBehaviour
{
    #region Variables
    [Header("Menu Pages")]
    [SerializeField] GameObject MainMenu;
    [SerializeField] GameObject Online;
    [SerializeField] GameObject LAN;
    [SerializeField] GameObject Settings;

    [Header("Bools")]
    private bool settingsOpen = true;
    private bool lanOpen = true;
    private bool onlineOpen = false;
    private bool host = false;
    private bool join = false;

    [Header("Scenes")]
    //[SerializeField] private string DevScn1Name;
    //[SerializeField] private string DevScn2Name;

    [Header("References")]
    private PlayerInputHandler input;
    private MenuNetworker networker;
    private Animator mainAnim;
    private Animator onlineAnim;
    private Animator lanAnim;
    private Animator settingsAnim;
    #endregion

    #region Default Stuff
    private void Start()
    {
        SetupComponents();

        lanOpen = false;
        LAN.SetActive(true);
        lanAnim.SetBool("Closed", true);

        onlineOpen = false;
        Online.SetActive(true);
        onlineAnim.SetBool("Closed", true);

        settingsOpen = false;
        Settings.SetActive(true);
        settingsAnim.SetBool("Closed", true);

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
        if (onlineAnim == null) onlineAnim = Online.GetComponent<Animator>();
        if (lanAnim == null) lanAnim = LAN.GetComponent<Animator>();
        if (settingsAnim == null) settingsAnim = Settings.GetComponent<Animator>();
    }

    public void CloseSubMenus()
    {
        if (lanOpen)
        {
            StartCoroutine(CloseLan());
        }

        if (settingsOpen)
        {
            StartCoroutine(CloseSettings());
        }

        if (onlineOpen)
        {
            StartCoroutine(CloseOnline());
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

        lanOpen = false;
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
        ipText.text = networker.GetLocalIPAddress();

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
        StartCoroutine(OpenOnline());
    }
    private IEnumerator OpenOnline()
    {
        mainAnim.SetBool("Closed", true);

        yield return new WaitForSeconds(1);

        onlineAnim.SetBool("Closed", false);
        onlineOpen = true;
    }

    private IEnumerator CloseOnline()
    {
        onlineAnim.SetBool("Closed", true);
        onlineAnim.SetBool("OpenH", false);
        onlineAnim.SetBool("OpenJ", false);

        yield return new WaitForSeconds(1);

        mainAnim.SetBool("Closed", false);

        onlineOpen = false;
    }

    public void OnlineHostPromptActivation()
    {
        if (!host) StartCoroutine(OnlineHostPromptOpen());
        else StartCoroutine(OnlineHostPromptClose());
    }

    private IEnumerator OnlineHostPromptOpen()
    {
        GameObject PopupHolder = LAN.transform.GetChild(1).gameObject;
        GameObject Popup = PopupHolder.transform.GetChild(0).gameObject;
        TMP_Text ipText = Popup.transform.GetChild(3).GetComponent<TMP_Text>();
        ipText.text = networker.GetLocalIPAddress();

        onlineAnim.SetBool("OpenH", true);
        yield return new WaitForSeconds(0.5f);

        host = true;
        join = false;

        onlineAnim.SetBool("OpenJ", false);
    }

    private IEnumerator OnlineHostPromptClose()
    {
        onlineAnim.SetBool("OpenH", false);
        yield return new WaitForSeconds(0.5f);

        host = false;
    }

    public void OnlineJoinPromptActivation()
    {
        if (!join) StartCoroutine(OnlineJoinPromptOpen());
        else StartCoroutine(OnlineJoinPromptClose());
    }

    private IEnumerator OnlineJoinPromptOpen()
    {
        GameObject PopupHolder = LAN.transform.GetChild(1).gameObject;
        GameObject Popup = PopupHolder.transform.GetChild(1).gameObject;

        onlineAnim.SetBool("OpenJ", true);
        yield return new WaitForSeconds(0.5f);

        join = true;
        host = false;

        onlineAnim.SetBool("OpenH", false);
    }

    private IEnumerator OnlineJoinPromptClose()
    {
        onlineAnim.SetBool("OpenJ", false);
        yield return new WaitForSeconds(0.5f);

        join = false;
    }

    public void HostOnline()
    {
        GameObject PopupHolder = Online.transform.GetChild(1).gameObject;
        GameObject Popup = PopupHolder.transform.GetChild(0).gameObject;
        int playerInput = int.Parse(Popup.transform.GetChild(3).GetComponent<TMP_InputField>().text);
        if (playerInput < 0 || playerInput == 0 || playerInput > 10) playerInput = 4;

        networker.HostGame(playerInput);
    }

    public void JoinOnline()
    {
        GameObject PopupHolder = Online.transform.GetChild(1).gameObject;
        GameObject Popup = PopupHolder.transform.GetChild(1).gameObject;
        string joinCode = Popup.transform.GetChild(3).GetComponent<TMP_InputField>().text;

        if (joinCode == null || joinCode.Length == 0) StartCoroutine(OnlineJoinPromptClose());
        else
        {
            networker.JoinGame(joinCode);
        }
    }

    #endregion

    #region Settings + Quit
    public void OpenSettingsMenu()
    {
        StartCoroutine(OpenSettings());
    }
    
    private IEnumerator OpenSettings()
    {
        mainAnim.SetBool("Closed", true);

        yield return new WaitForSeconds(1);

        settingsAnim.SetBool("Closed", false);
        settingsOpen = true;
    }

    private IEnumerator CloseSettings()
    {
        settingsAnim.SetBool("Closed", true);

        yield return new WaitForSeconds(.25f);

        mainAnim.SetBool("Closed", false);

        settingsOpen = false;
    }

    public void MainMenuQuit()
    {
        Application.Quit();
    }
    #endregion
}
