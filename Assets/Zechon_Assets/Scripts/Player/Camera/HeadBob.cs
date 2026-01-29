using UnityEngine;

public class HeadBob : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float amplitude = 0.035f;
    [SerializeField] float frequency = 7.5f;

    [Header("References")]
    [SerializeField] PlayerMovement movement;
    [SerializeField] PlayerInputHandler input;

    private float phase;
    private float currentOffset;
    private float offsetVelocity;

    private void LateUpdate()
    {
        bool shouldBob = movement.state != PlayerMovement.MovementState.Airborne && input.Move.sqrMagnitude > 0.01f;

        if (shouldBob)
        {
            float speedFactor = movement.state == PlayerMovement.MovementState.Sprinting
                ? 1.3f
                : 1f;

            phase += Time.deltaTime * frequency * speedFactor;

            float targetOffset = Mathf.Sin(phase) * amplitude;

            currentOffset = Mathf.SmoothDamp(currentOffset, targetOffset, ref offsetVelocity, 0.08f);
        }
        else
        {
            currentOffset = Mathf.SmoothDamp(currentOffset, 0f, ref offsetVelocity, 0.12f);
        }

        transform.localPosition = Vector3.up * currentOffset;
    }
}
