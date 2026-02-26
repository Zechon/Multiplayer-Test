using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerLocalInteractionsSetup : MonoBehaviour
{
    [Header("Enable Interactions")]
    [SerializeField] private bool pauseMenuInteract;

    public float pause_Stopwatch = 0f;
    private bool pause_NullOrNot = true;
    private bool pause_SetupRun = false;

    [Header("Objects / Refs")]
    [SerializeField] private PlayerCamera plyrCamRef;
    [SerializeField] private MenuNetworker ntwk;
    private GameObject pauseObject;
    private PauseMenuHandler pauseMenuHandler;

    //Debug Log Output String
    [HideInInspector]public string debugString = "";

    private IEnumerator PauseMenuSetup()
    {
        pause_SetupRun = true;

        debugString += "Pause Menu Object: ";

        if (pauseObject == null) debugString += "NULL / Not Found";

        else debugString += $"{pauseObject.name}";

        debugString += "\n\tPause Menu: ";
        

       pauseMenuHandler = pauseObject.GetComponent<PauseMenuHandler>();

       pauseMenuHandler.Setup();

        if (pauseMenuHandler.setup == true)
        {
            debugString += "Active";

            plyrCamRef.PauseHSetup(pauseMenuHandler);
            transform.GetComponent<PlayerDebugger>().pauseHSetup(pauseMenuHandler);
            yield break;
        }

        else
        {
            debugString += "Setup Failed";
            yield break;
        }
    }

    private void Update()
    {
        if (pauseMenuInteract)
        {
            if (pause_NullOrNot == true && pause_SetupRun == false)
            {
                pauseObject = GameObject.FindGameObjectWithTag("Pause");
                if (pauseObject == null)
                {
                    pause_Stopwatch += Time.deltaTime;
                }

                else
                {
                    pause_NullOrNot = false;
                }
            }

            if (pause_SetupRun == false && pause_NullOrNot == false)
            {
                StartCoroutine(PauseMenuSetup());
            }
        }

        else if (!pauseMenuInteract && pause_SetupRun == false)
        {
            pauseMenuHandler = null;
            debugString += "Pause Menu Setup: Not Enabled";
            pause_SetupRun = true;
        }
    }
}
