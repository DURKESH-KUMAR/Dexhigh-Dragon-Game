using UnityEngine;

public class TopDownCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Position Settings")]
    public Vector3 offset = new Vector3(0f, 10f, -8f);
    public float followSmoothTime = 0.15f;
    public float verticalFollowStrength = 0.5f;

    [Header("Rotation Settings")]
    public float tiltAngle = 45f;

    private Vector3 currentVelocity;

    void LateUpdate()
    {
        if (!target) return;

        // Adjust Y offset when flying (soft follow)
        Vector3 dynamicOffset = offset;
        dynamicOffset.y += target.position.y * verticalFollowStrength;

        Vector3 desiredPosition = target.position + dynamicOffset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref currentVelocity,
            followSmoothTime
        );

        // Lock rotation (2.5D style)
        transform.rotation = Quaternion.Euler(tiltAngle, 0f, 0f);
    }
}
