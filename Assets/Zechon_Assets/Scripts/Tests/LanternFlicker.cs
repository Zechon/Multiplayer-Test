using UnityEngine;

[RequireComponent(typeof(Light))]
public class LanternLightFlicker : MonoBehaviour
{
    [Header("Flicker Settings")]
    public float minIntensity = 0.6f;
    public float maxIntensity = 1.2f;

    [Tooltip("Higher = smoother flicker, lower = more jittery.")]
    public float smoothing = 10f;

    [Tooltip("Speed of flicker randomness.")]
    public float flickerSpeed = 20f;

    private Light lanternLight;
    private float targetIntensity;

    private void Awake()
    {
        lanternLight = GetComponent<Light>();
        lanternLight.intensity = maxIntensity;
    }

    private void Update()
    {
        // Random flicker target
        targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, Mathf.PerlinNoise(Time.time * flickerSpeed, 0f));

        // Smooth transition
        lanternLight.intensity = Mathf.Lerp(lanternLight.intensity, targetIntensity, Time.deltaTime * smoothing);
    }
}
