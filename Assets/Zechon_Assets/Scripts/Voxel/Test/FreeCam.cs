using UnityEngine;

public class FreeCam : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float fastSpeed = 30f;
    public float slowSpeed = 3f;

    [Header("Mouse Look")]
    public float lookSensitivity = 2f;
    public bool lockCursor = true;

    float _yaw;
    float _pitch;

    void Start()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        _yaw = rot.y;
        _pitch = rot.x;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        _yaw += mouseX;
        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, -89f, 89f);

        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }

    void HandleMovement()
    {
        float speed = moveSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
            speed = fastSpeed;
        else if (Input.GetKey(KeyCode.LeftControl))
            speed = slowSpeed;

        Vector3 dir = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) dir += transform.forward;
        if (Input.GetKey(KeyCode.S)) dir -= transform.forward;
        if (Input.GetKey(KeyCode.A)) dir -= transform.right;
        if (Input.GetKey(KeyCode.D)) dir += transform.right;
        if (Input.GetKey(KeyCode.E)) dir += transform.up;
        if (Input.GetKey(KeyCode.Q)) dir -= transform.up;

        transform.position += dir * speed * Time.deltaTime;
    }
}
