using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class shopGuyDialogues : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI shopGuyText;

    private readonly List<string> startingDialogue = new List<string>
    {
        "Hey there! I'm the shop guy in the area..",
        "Seems like we are working for the same guy..",
        "You know? The black creepy dude? Yeah..",
        "He scares me...",
        "Anyways, the rules are simple",
        "First pay your taxes, then buy from me, not the other way around..",
        ""
    };

    private readonly List<string> cantOpenDialogue = new List<string>
    {
        "I don't make up the rules.. Pay your taxes please..",
        "I can't tax evade... he is scary....",
        "Nope.. You know the drill..",
        "Forgetting something?..."
    };


    public bool hasStartedFirstDialogue { get; private set; } = false;
    public bool firstDialogueFinished { get; private set; } = false;
    public bool ShopGuyIsTalking { get; private set; } = false;

    private Coroutine dialogueRoutine;

    void Start()
    {
        if (shopGuyText == null)
        {
            Debug.LogError("❌ shopGuyText reference is missing on " + name);
            enabled = false;
            return;
        }

        shopGuyText.text = "";
    }


    private void Update()
    {
        // ✅ Only run this if text is in world space (avoid unnecessary per-frame cost)
        if (shopGuyText.canvas.renderMode == RenderMode.WorldSpace)
        {
            Vector3 worldOffset = new Vector3(0, 2f, 0);
            shopGuyText.transform.position = transform.position + worldOffset;
        }
    }
    public void StartDialogue()
    {
        if (ShopGuyIsTalking || dialogueRoutine != null)
            return; // ✅ Prevent overlapping coroutines

        hasStartedFirstDialogue = true;
        dialogueRoutine = StartCoroutine(DisplayStartingSpeech());
    }

    private IEnumerator DisplayStartingSpeech()
    {
        ShopGuyIsTalking = true;

        foreach (string line in startingDialogue)
        {
            shopGuyText.text = line;

            // Wait for E press/release
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.E));
            yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.E));
        }

        firstDialogueFinished = true;
        EndDialogue();
    }

    public void CantOpenShopDialogue()
    {
        if (ShopGuyIsTalking)
            return;

        int randomIndex = Random.Range(0, cantOpenDialogue.Count);
        shopGuyText.text = cantOpenDialogue[randomIndex];
        ShopGuyIsTalking = true;
    }

    public void HideDialogue()
    {
        if (!ShopGuyIsTalking)
            return;

        EndDialogue();
    }

    private void EndDialogue()
    {
        ShopGuyIsTalking = false;
        shopGuyText.text = "";
        if (dialogueRoutine != null)
        {
            StopCoroutine(dialogueRoutine);
            dialogueRoutine = null;
        }
    }
}
