using UnityEngine;

public class camerafollow : MonoBehaviour
{
    public Transform target;          // The object to follow
    public Vector3 offset;            // Offset from the target
    public float smoothTime = 0.2f;   // Smaller = snappier, Larger = smoother

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        FollowTarget();
    }

    void FollowTarget()
    {
        // Target position with offset
        Vector3 targetPosition = target.position + offset;
        targetPosition.z = transform.position.z; // Keep Z for 2D

        // Smoothly move the camera
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
