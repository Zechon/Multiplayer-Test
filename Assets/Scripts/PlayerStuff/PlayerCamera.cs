using UnityEngine;
using Unity.Netcode;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] private Camera playerCam;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private AudioListener listener;
    private Camera mainCam;
    private AudioListener mainListener;
    private float xRotation;

    void Start()
    {
        GameObject mainObj = GameObject.FindGameObjectWithTag("MainCamera");
        mainCam = mainObj.GetComponent<Camera>();
        mainCam.enabled = false;
        mainListener = mainObj.GetComponent<AudioListener>();
        mainListener.enabled = false;

        if (!IsOwner)
        {
            playerCam.enabled = false;
            listener.enabled = false;
        }
        else 
            Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (!IsOwner) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);

        playerCam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }
}
