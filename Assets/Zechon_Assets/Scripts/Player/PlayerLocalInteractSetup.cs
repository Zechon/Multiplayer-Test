using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerLocalInteractionsSetup : MonoBehaviour
{
    [Header("Enable Interactions")]
    [SerializeField] private bool pauseMenuInteract;
    private PauseMenuHandler pauseMenuHandler;

    //Debug Log Output String
    private string debugString = "";

    public void OnAwake()
    {
        if (pauseMenuInteract)
        {
            debugString += "Pause Menu:\n";
            GameObject pauseObject = GameObject.Find("PauseMenuHandler");
            if (pauseObject == null) pauseObject = GameObject.FindGameObjectWithTag("Pause");
            if (pauseObject == null) debugString += "\n\tPause Menu Object: NULL";
            else debugString += $"\n\tPause Menu Object: {pauseObject.name}";

            if (pauseObject != null)
            {
                pauseMenuHandler.Setup();
                pauseMenuHandler = pauseObject.GetComponent<PauseMenuHandler>();
                if (pauseMenuHandler.setup == true) debugString += "\t|\tPause Menu: Active";
                return;
            }
            
            debugString += "\t|\tPause Menu: Setup Failed";
        }

        else
        {
            pauseMenuHandler = null;
            debugString += "\tPause Menu: Inactive";
        }

        //GameDebugRegistry.Register(BuildSection);
    }

    private void OnDisable()
    {
        //GameDebugRegistry.Unregister(BuildSection);
    }
}
