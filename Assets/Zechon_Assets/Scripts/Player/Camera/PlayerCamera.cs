using UnityEngine;
using Unity.Netcode;

public class PlayerCamera : NetworkBehaviour
{
    [Header("Sensitivity")]
    [SerializeField] float sensX;
    [SerializeField] float sensY;

    [Header("References")]
    [SerializeField] private Camera playerCam;
    [SerializeField] private AudioListener listener;
    [SerializeField] Transform orientation;
    [SerializeField] Transform cameraPivot;
    [SerializeField] private PauseMenuHandler pauseHandler;

    //Other
    private Camera mainCam;
    private AudioListener mainListener;
    private float pitch;

    void Start()
    {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        mainListener = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<AudioListener>();
        mainCam.enabled = false;
        mainListener.enabled = false;

        pauseHandler = GameObject.FindGameObjectWithTag("Pause").GetComponent<PauseMenuHandler>();

        if (!IsOwner)
        {
            playerCam.enabled = false;
            listener.enabled = false;
        }
        else
            CursorLocker.Lock();
}

    private void LateUpdate()
    {
        if (!IsOwner) return;

        if (pauseHandler != null && pauseHandler.paused) return;

        float mouseX = Input.GetAxis("Mouse X") * sensX;
        float mouseY = Input.GetAxis("Mouse Y") * sensY;

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -85f, 85f);

        cameraPivot.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
        orientation.Rotate(Vector3.up * mouseX);
    }
}
