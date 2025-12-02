using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bogdanosInteractions : MonoBehaviour
{
    [Header("References")]
    public GameObject player;
    public GameObject countdownTimerObj;

    private coins c;
    private bogdanosDialogue dialogue;
    private countdowntimer countdown;
    private GemTaken takenGem;

    [Header("Settings")]
    public float interactionRange = 3f;
    public KeyCode interactKey = KeyCode.E;

    private int sumOfTalks = 0;

    void Start()
    {
        if (!player || !countdownTimerObj)
        {
            Debug.LogError("Player or CountdownTimerObj not assigned!");
            enabled = false;
            return;
        }

        takenGem = player.GetComponent<GemTaken>();
        dialogue = GetComponent<bogdanosDialogue>();
        countdown = countdownTimerObj.GetComponent<countdowntimer>();
        c = player.GetComponent<coins>();
    }

    void Update()
    {
        float distance = Vector2.Distance(transform.position, player.transform.position);

        // Hide dialogue if player moves too far
        if (distance > interactionRange * 1.5f)
        {
            dialogue.HideDialogue();
            return;
        }

        // Only proceed if player is in range and presses the key
        if (distance <= interactionRange && Input.GetKeyDown(interactKey))
        {
            // Player must not have taken the gem
            if (takenGem.GemIsTaken)
                return;

            // First talk
            if (sumOfTalks == 0)
            {
                dialogue.GetStartingText();
            }
            // Subsequent talks: must have coins
            else if (c.beforetaxCoins > 0)
            {
                dialogue.printBogdanosMoneyText();
                c.bogdanosTax();
                countdown.timer = 320f;
                countdown.TriggerGreenEffect();
            }

            sumOfTalks++;
        }
    }
}
