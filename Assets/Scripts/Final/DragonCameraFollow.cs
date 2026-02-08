using UnityEngine;

public class DragonCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Offset")]
    public Vector3 offset = new Vector3(0f, 6f, -10f);

    [Header("Follow Settings")]
    public float followSpeed = 8f;
    public float lookAheadDistance = 2f;
    public float verticalLookAhead = 1.5f;

    [Header("Clamp (Optional)")]
    public bool useClamp = false;
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private Vector3 lastPlayerPos;
    private Vector3 lookAhead;

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("DragonCameraFollow: Player not assigned!");
            enabled = false;
            return;
        }

        lastPlayerPos = player.position;
    }

    void LateUpdate()
    {
        Vector3 playerDelta = player.position - lastPlayerPos;

        // Horizontal look ahead (platformer feel)
        lookAhead.x = Mathf.Lerp(
            lookAhead.x,
            Mathf.Sign(playerDelta.x) * lookAheadDistance,
            Time.deltaTime * 5f
        );

        // Vertical look ahead (fly / jump)
        lookAhead.y = Mathf.Lerp(
            lookAhead.y,
            playerDelta.y * verticalLookAhead,
            Time.deltaTime * 5f
        );

        Vector3 targetPos = player.position + offset + lookAhead;

        // Clamp camera if needed
        if (useClamp)
        {
            targetPos.x = Mathf.Clamp(targetPos.x, minBounds.x, maxBounds.x);
            targetPos.y = Mathf.Clamp(targetPos.y, minBounds.y, maxBounds.y);
        }

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            followSpeed * Time.deltaTime
        );

        lastPlayerPos = player.position;
    }
}
