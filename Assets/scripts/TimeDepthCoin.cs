using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeDepthCoin : MonoBehaviour
{
    public Text maxdepthText;
    public Text maxCoinText;
    public Text maxTimeText;

    public float animationDuration = 1.5f; // Time it takes to animate to final value

    void Start()
    {
        int t = timerScript.time;
        int hours = t / 3600;
        int minutes = (t % 3600) / 60;
        int seconds = t % 60;

        int dep = -1 * showDepth.GetMinDepth();
        


        // Start coroutines to animate values
        StartCoroutine(AnimateDepth(dep));
        StartCoroutine(AnimateCoins(coins.maxcoins));
        StartCoroutine(AnimateTime(t));
    }

    IEnumerator AnimateDepth(int finalDepth)
    {
        float elapsed = 0;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / animationDuration);
            int currentDepth = Mathf.RoundToInt(Mathf.Lerp(0, finalDepth, progress));
            maxdepthText.text = currentDepth + " m";
            yield return null;
        }
        maxdepthText.text = finalDepth + " m";
    }

    IEnumerator AnimateCoins(int finalCoins)
    {
        float elapsed = 0;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / animationDuration);
            int currentCoins = Mathf.RoundToInt(Mathf.Lerp(0, finalCoins, progress));
            maxCoinText.text = currentCoins.ToString();
            yield return null;
        }
        maxCoinText.text = finalCoins.ToString();
    }

    IEnumerator AnimateTime(int finalSeconds)
    {
        float elapsed = 0;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / animationDuration);
            int currentSeconds = Mathf.RoundToInt(Mathf.Lerp(0, finalSeconds, progress));

            int hours = currentSeconds / 3600;
            int minutes = (currentSeconds % 3600) / 60;
            int seconds = currentSeconds % 60;

            maxTimeText.text = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
            yield return null;
        }

        // Set final value
        int finalHours = finalSeconds / 3600;
        int finalMinutes = (finalSeconds % 3600) / 60;
        int finalSecs = finalSeconds % 60;
        maxTimeText.text = $"{finalHours:D2}:{finalMinutes:D2}:{finalSecs:D2}";
    }
}
