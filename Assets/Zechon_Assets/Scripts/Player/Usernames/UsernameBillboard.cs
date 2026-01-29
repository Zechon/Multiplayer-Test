using UnityEngine;

public class UsernameBillboard : MonoBehaviour
{
    private Camera mainCamera;
    private GameObject[] nametags;
    private float updateTimer;

    private void Start()
    {
        InvokeRepeating(nameof(FindMainCamera), 0f, 1f);
    }

    private void FindMainCamera()
    {
        if (mainCamera == null)
        {
            Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (var cam in cameras)
            {
                if (cam.isActiveAndEnabled && cam.CompareTag("PlayerCamera"))
                {
                    mainCamera = cam;
                    break;
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
            return;

        updateTimer += Time.deltaTime;
        if (updateTimer >= 0.5f)
        {
            nametags = GameObject.FindGameObjectsWithTag("nametag");
            updateTimer = 0f;
        }

        if (nametags == null || nametags.Length == 0)
            return;

        foreach (var nametag in nametags)
        {
            if (nametag == null) continue;

            Vector3 lookDirection = nametag.transform.position - mainCamera.transform.position;
            lookDirection.y = 0f;

            if (lookDirection.sqrMagnitude > 0.001f)
            {
                nametag.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }
}
