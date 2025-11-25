using Unity.Netcode;
using UnityEngine;

public class DayNight : NetworkBehaviour
{
    [Header("Sun Settings")]
    public Light sunLight;
    [Range(0f, 1f)] public float nightIntensity = 0.05f; // minimum sun intensity at night
    [Range(0f, 2f)] public float dayIntensity = 1f;      // maximum sun intensity at noon

    [Header("Time Settings")]
    [Tooltip("Length of full day in seconds (sunrise -> next sunrise)")]
    public float dayLengthSeconds = 300f; // 5 minutes
    private float localTime = 0f;          // 0-1 normalized time

    private NetworkVariable<float> networkTime = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    [Header("Ambient & Fog")]
    public Color nightAmbient = new Color(0.02f, 0.02f, 0.05f);
    public Color dayAmbient = Color.gray;
    public Color nightFog = new Color(0.02f, 0.02f, 0.05f);
    public Color dayFog = new Color(0.5f, 0.6f, 0.7f);
    public float maxFogDensity = 0.02f;
    public float minFogDensity = 0.005f;

    // Smoothing for clients
    private float smoothedTime;

    private void Start()
    {
        // Subscribe to network variable changes
        networkTime.OnValueChanged += OnTimeChanged;

        // Initialize smoothedTime
        smoothedTime = networkTime.Value;
    }

    private void Update()
    {
        if (IsOwner) // Host controls time
        {
            localTime += Time.deltaTime / dayLengthSeconds;
            localTime %= 1f;
            networkTime.Value = localTime;
        }

        // Smoothly interpolate the time for all clients
        smoothedTime = Mathf.Lerp(smoothedTime, networkTime.Value, Time.deltaTime * 5f);
        ApplyDayNight(smoothedTime);
    }

    private void OnTimeChanged(float oldTime, float newTime)
    {
        // Immediate update if needed (optional)
        smoothedTime = newTime;
    }

    private void ApplyDayNight(float t)
    {
        // Sun rotation: 0 = sunrise, 90 = noon, 180 = sunset
        float sunAngle = t * 360f - 90f; // -90 so 0 is sunrise
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 45f, 0f);

        // Intensity peaks at noon (sunAngle = 90)
        float intensityMultiplier = Mathf.Clamp01(Mathf.Cos((sunAngle - 90f) * Mathf.Deg2Rad));
        sunLight.intensity = Mathf.Lerp(nightIntensity, dayIntensity, intensityMultiplier);

        // Ambient light
        RenderSettings.ambientLight = Color.Lerp(nightAmbient, dayAmbient, intensityMultiplier);

        // Fog color
        RenderSettings.fogColor = Color.Lerp(nightFog, dayFog, intensityMultiplier);

        // Fog density
        RenderSettings.fogDensity = Mathf.Lerp(maxFogDensity, minFogDensity, intensityMultiplier);
    }
}
