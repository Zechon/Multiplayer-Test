using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerDebugger : NetworkBehaviour
{
    [Header("Debug References")]
    [SerializeField] private TMP_Text username;
    [SerializeField] private CharacterController charController;
    [SerializeField] private PlayerMovement plyrMovement;
    [SerializeField] private GameObject cameraPivot;
    [SerializeField] private GameObject orientation;
    [SerializeField] private PlayerLocalInteractionsSetup plyrLocalInteract;
    private PauseMenuHandler pause;

    private string debugKey;

    private Vector2 prevPos;

    public override void OnNetworkSpawn()
    {
        debugKey = "Player_" + NetworkObjectId;

        prevPos = new Vector2(transform.position.x, transform.position.z);

        GameDebugRegistry.Register(debugKey, BuildSection);
    }

    private void OnDisable()
    {
        if (!string.IsNullOrEmpty(debugKey))
            GameDebugRegistry.Unregister(debugKey); ;
    }

    private DebugSection BuildSection()
    {
        DebugSection root = new DebugSection(
            debugKey,
            $"Player: {username.text}",
            "",
            10
        );

        root.Children.Add(new DebugSection(
            debugKey + "_movement",
            "Movement",
            $"Horizontal Velocity: {CalcVelocity()}\n" +
            $"Vertical Velocity: {charController.velocity.y.ToString("+0.00;-0.00")}\n" +
            $"Movement State: {plyrMovement.state}\n" +
            $"Grounded: {plyrMovement.grounded}",
            0
        ));

        root.Children.Add(new DebugSection(
            debugKey + "_position",
            "Transform",
            $"Position: {transform.position}\n" +
            $"Camera Rotation: X {cameraPivot.transform.eulerAngles.x.ToString("+0.00;-0.00")}, Y {orientation.transform.eulerAngles.y.ToString("+0.00;-0.00")}",
            1
        ));

        root.Children.Add(new DebugSection(
            debugKey + "_components",
            "Components",
            $"Pause Menu\n" +
            $"\t{plyrLocalInteract.debugString}\n" +
            $"\tPause Menu Init Time: {plyrLocalInteract.pause_Stopwatch.ToString("+0.00;-0.00")}",
            1
        ));

        root.Children.Add(new DebugSection(
            debugKey + "_gameInfo",
            "Game Info",
            $"Paused: {pause.paused}" +
            $"",
            1
        ));

        return root;
    }

    private string CalcVelocity()
    {
        float distanceTraveled = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), prevPos);

        string result = (distanceTraveled / Time.deltaTime).ToString("+0.00;-0.00");

        prevPos = new Vector2(transform.position.x, transform.transform.position.z);

        return result;
    }

    public void pauseHSetup(PauseMenuHandler pmh)
    {
        pause = pmh;
    }
}
