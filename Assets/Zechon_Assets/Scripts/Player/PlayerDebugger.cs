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

    private string debugKey;

    public override void OnNetworkSpawn()
    {
        debugKey = "Player_" + NetworkObjectId;

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
            $"Velocity: {charController.velocity}\n" + 
            $"State: {plyrMovement.state}\n" +
            $"Grounded?: {plyrMovement.grounded}",
            0
        ));

        root.Children.Add(new DebugSection(
            debugKey + "_position",
            "Transform",
            $"Position: {transform.position}\n" +
            $"Camera Rotation: {cameraPivot.transform.localRotation}",
            1
        ));

        return root;
    }
}
