using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class countdowntimer : MonoBehaviour
{
    public Text countdownText;
    public float timer = 320f;
    private float lastSecond = -1f;

    private Color defaultColor;
    private Vector3 defaultScale;
    private bool isFlashing = false;

    void Start()
    {
        if (countdownText == null)
            countdownText = GetComponent<Text>();

        defaultColor = countdownText.color;
        defaultScale = transform.localScale;

        UpdateCountdownText();
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                timer = 0;
                Debug.Log("You lost! Time ran out.");
                // GameOver() or load a new scene could go here
            }

            int currentSecond = Mathf.FloorToInt(timer);

            if (currentSecond != lastSecond)
            {
                lastSecond = currentSecond;
                UpdateCountdownText();

                // Flash red only when timer <= 60 seconds
                if (timer <= 60)
                {
                    StartCoroutine(FlashRed());
                }
            }
        }

        // When the timer gets reset to 320f externally, trigger effect
        if (!isFlashing && Mathf.Approximately(timer, 320f))
        {
            StartCoroutine(FlashGreenAndZoom());
        }
    }

    void UpdateCountdownText()
    {
        int minutes = Mathf.FloorToInt(timer / 60);
        int seconds = Mathf.FloorToInt(timer % 60);
        countdownText.text = $"{minutes:00}:{seconds:00}";
    }

    IEnumerator FlashRed()
    {
        countdownText.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        countdownText.color = defaultColor;
    }

    IEnumerator FlashGreenAndZoom()
    {
        isFlashing = true;

        float duration = 0.7f; // total animation time
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Smooth zoom in/out using sine curve
            float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.2f;
            transform.localScale = defaultScale * scale;

            // Smooth color fade to green and back
            if (t < 0.5f)
                countdownText.color = Color.Lerp(defaultColor, Color.green, t * 2f);
            else
                countdownText.color = Color.Lerp(Color.green, defaultColor, (t - 0.5f) * 2f);

            yield return null;
        }

        transform.localScale = defaultScale;
        countdownText.color = defaultColor;
        isFlashing = false;
    }

    // Optional: a public method you can call from other scripts to trigger the effect manually
    public void TriggerGreenEffect()
    {
        if (!isFlashing)
            StartCoroutine(FlashGreenAndZoom());
    }
}
