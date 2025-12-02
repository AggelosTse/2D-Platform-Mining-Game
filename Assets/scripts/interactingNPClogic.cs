using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class interactingNPClogic : MonoBehaviour
{
    [Header("NPC References")]
    public GameObject shopGobj;
    public GameObject bogdObj;

    private shopGuyDialogues shopGdial;
    private bogdanosDialogue bogdDial;

    [Header("Camera")]
    public GameObject camObj;
    private camerafollow cam;

    [Header("Player Components")]
    private Rigidbody2D rb;
    private movement playerMovement;

    // ✅ Cached state
    private bool lastTalkingState = false;
    private Transform lastCamTarget;

    void Start()
    {
        // Cache components once
        shopGdial = shopGobj.GetComponent<shopGuyDialogues>();
        bogdDial = bogdObj.GetComponent<bogdanosDialogue>();
        cam = camObj.GetComponent<camerafollow>();
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<movement>();

        lastCamTarget = transform;
    }

    void Update()
    {
        HandleInteractions();
    }

    void HandleInteractions()
    {
        bool isTalking = shopGdial.ShopGuyIsTalking || bogdDial.bogdanosIsTalking;

        // ✅ Only freeze/unfreeze when state changes
        if (isTalking != lastTalkingState)
        {
            if (isTalking)
                FreezePlayer();
            else
                UnfreezePlayer();

            lastTalkingState = isTalking;
        }

        // ✅ Only change camera target when necessary
        Transform desiredTarget = GetDesiredCameraTarget();

        if (desiredTarget != lastCamTarget)
        {
            cam.target = desiredTarget;
            lastCamTarget = desiredTarget;
        }
    }

    Transform GetDesiredCameraTarget()
    {
        if (shopGdial.ShopGuyIsTalking ||
            (shopGdial.hasStartedFirstDialogue && !shopGdial.firstDialogueFinished))
        {
            return shopGobj.transform;
        }
        else if (bogdDial.bogdanosIsTalking)
        {
            return bogdObj.transform;
        }
        else
        {
            return transform; // Back to player
        }
    }

    void FreezePlayer()
    {
        // Stop movement safely without over-freezing physics
        playerMovement.enabled = false;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
    }

    void UnfreezePlayer()
    {
        // Restore normal movement and physics
        playerMovement.enabled = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
}
