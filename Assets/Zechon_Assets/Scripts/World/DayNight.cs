using Unity.Netcode;
using UnityEngine;

public class DayNight : NetworkBehaviour
{
    [Header("Sun & Moon")]
    public Light sunLight;
    [Range(0f, 1f)] public float dayNightIntensityMin = 0.05f; // how dark night is
    [Range(0f, 2f)] public float sunIntensityMax = 1f;

    [Header("Time Settings")]
    [Tooltip("How long a full day (sunrise to next sunrise) takes in seconds")]
    public float dayLengthSeconds = 300f; // 5 minutes for full day
    private float localTime = 0f; // 0-1 normalized time
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

    private void Update()
    {
        if (IsOwner) // Only host advances time
        {
            localTime += Time.deltaTime / dayLengthSeconds;
            localTime %= 1f;
            networkTime.Value = localTime;
        }

        // Apply rotation and lighting for all clients
        ApplyDayNight(networkTime.Value);
    }

    private void ApplyDayNight(float t)
    {
        // Sun rotation: 0 = sunrise, 180 = sunset, 90 = noon
        float sunAngle = t * 360f - 90f; // same as before
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 45f, 0f);

        // Intensity multiplier peaks at noon (x = 90°)
        // Map t = 0.25 (90°) -> cos(0) = 1
        float intensityMultiplier = Mathf.Clamp01(Mathf.Cos((sunAngle - 90f) * Mathf.Deg2Rad));

        // Sun intensity
        sunLight.intensity = Mathf.Lerp(dayNightIntensityMin, sunIntensityMax, intensityMultiplier);

        // Ambient light
        RenderSettings.ambientLight = Color.Lerp(nightAmbient, dayAmbient, intensityMultiplier);

        // Fog color
        RenderSettings.fogColor = Color.Lerp(nightFog, dayFog, intensityMultiplier);

        // Fog density
        RenderSettings.fogDensity = Mathf.Lerp(0.02f, 0.005f, intensityMultiplier);
    }


}
