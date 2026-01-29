using UnityEngine;

public class CameraCrouchSync : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CharacterController controller;
    [SerializeField] Transform cameraPivot;

    [Header("Settings")]
    [SerializeField] float cameraHeightOffset = 0.5f;
    [SerializeField] float smoothSpeed = 12f;

    private float velocity;

    private void LateUpdate()
    {
        float targetY = controller.center.y + controller.height / 2f - cameraHeightOffset;

        Vector3 localPos = cameraPivot.localPosition;
        localPos.y = Mathf.SmoothDamp(localPos.y, targetY, ref velocity, 1f / smoothSpeed);

        cameraPivot.localPosition = localPos;
    }
}
