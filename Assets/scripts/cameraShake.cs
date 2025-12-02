using UnityEngine;

public class cameraShake : MonoBehaviour
{
    public float duration;
    public float magnitude;
    public bool shakeOnStart = false;

    private Vector3 originalPos;
    private float elapsed = 0f;
    private bool isShaking = false;

    void Start()
    {
        originalPos = transform.localPosition;
        if (shakeOnStart)
        {
            StartShake();
        }
    }

    void Update()
    {
        if (isShaking)
        {
            elapsed += Time.deltaTime;

            if (elapsed < duration)
            {
                // Diagonal shake using sinusoidal offset
                float x = Mathf.Sin(elapsed * 30f) * magnitude;
                float y = x; // Same value for diagonal movement (x = y)

                transform.localPosition = originalPos + new Vector3(x, y, 0f);
            }
            else
            {
                isShaking = false;
                elapsed = 0f;
                transform.localPosition = originalPos;
            }
        }
    }

    public void StartShake(float shakeDuration = -1f, float shakeMagnitude = -1f)
    {
        if (shakeDuration > 0) duration = shakeDuration;
        if (shakeMagnitude > 0) magnitude = shakeMagnitude;

        originalPos = transform.localPosition;
        elapsed = 0f;
        isShaking = true;
    }
}
