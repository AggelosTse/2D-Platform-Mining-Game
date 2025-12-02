using UnityEngine;

public class shopGuyInteractions : MonoBehaviour
{
    [Header("References")]
    public GameObject player;

    private coins c;
    private shopGuyDialogues dialogues;

    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Shop State")]
    public bool canOpenShop = false;
    private int sumOfTalks = 0;

    void Start()
    {
        if (!player)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (!player)
            {
                Debug.LogError("Player not assigned and no GameObject with tag 'Player' found!");
                enabled = false;
                return;
            }
        }

        dialogues = GetComponent<shopGuyDialogues>();
        c = player.GetComponent<coins>();

        if (!dialogues)
            Debug.LogError("shopGuyDialogues component missing from shopGuyInteractions object!");
        if (!c)
            Debug.LogError("coins component missing from Player object!");
    }

    void Update()
    {
        float distance = Vector2.Distance(player.transform.position, transform.position);

        // Hide dialogue if player moves too far
        if (distance > interactionRange * 1.5f)
        {
            dialogues.HideDialogue();
            canOpenShop = false;
            return;
        }

        // Player presses interact key while in range
        if (distance <= interactionRange && Input.GetKeyDown(interactKey))
        {
            HandleInteraction();
        }
        else
        {
            canOpenShop = false;
        }
    }

    private void HandleInteraction()
    {
        // First talk
        if (sumOfTalks == 0)
        {
            dialogues.StartDialogue();
            sumOfTalks++;
            return;
        }

        // Subsequent talks
        if (c.beforetaxCoins <= 0)
        {
            canOpenShop = true;
            dialogues.HideDialogue(); // Optional: close any dialogue before opening shop
        }
        else
        {
            canOpenShop = false;
            dialogues.CantOpenShopDialogue();
        }

        sumOfTalks++;
    }
}
