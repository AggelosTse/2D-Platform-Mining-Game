using UnityEngine;
using System.Collections;

public class wallClimb : MonoBehaviour
{
    public float climbForce = 8f;
    public GameObject wallSensor; // Assumed to handle wall proximity (IsTouchingWall)
    public float handDisplayDuration = 0.5f;
    public GameObject hands;
    // NEW: Variable to control the wall stick/slide state
    private bool isStuckToWall = false;

    private bool isTouchingWall = false;
    private bool isShowingHands = false;
    private Rigidbody2D rb;
    SpriteRenderer sprite;

    movement mo;
    Animator animator;
    public bool isclimbing;

    private void Awake()
    {
        if (hands != null)
        {
            sprite = hands.GetComponent<SpriteRenderer>();
        }
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        isclimbing = false;
        mo = GetComponent<movement>();
        if (sprite != null)
        {
            sprite.enabled = false;
        }
    }

    private void Update()
    {
        // ----------------------------------------------------
        // WALL CLIMB LOGIC (Upward movement with Spacebar)
        // ----------------------------------------------------
        // Check for: Wall touch, Space key, AND upward movement
        if (isTouchingWall && Input.GetKeyDown(KeyCode.Space) && rb.velocity.y > 0)
        {
            // If we were stuck, unstuck before climbing
            if (isStuckToWall)
            {
                ToggleWallStick(false);
            }

            animator.enabled = false;
            isclimbing = true;

            if (mo != null && sprite != null)
            {
                if (mo.lookingright)
                    sprite.flipX = false;
                else
                    sprite.flipX = true;
            }

            rb.velocity = new Vector2(rb.velocity.x, climbForce);

            if (!isShowingHands)
            {
                StartCoroutine(ShowHandsForDuration());
            }
        }
        // ----------------------------------------------------
        // WALL STICK LOGIC (Downward movement with R key)
        // ----------------------------------------------------
        else if (isTouchingWall && Input.GetMouseButtonDown(1))
        {
            // If the player is currently stuck, unstick them
            if (isStuckToWall)
            {
                ToggleWallStick(false);
            }
            // If the player is NOT stuck AND is moving downwards, stick them
            else if (rb.velocity.y <= 0) // Checks if moving downwards or stationary
            {
                ToggleWallStick(true);
            }
        }

        // APPLY STICK EFFECT: If stuck, set the vertical velocity to zero (or a low slide value)
        if (isStuckToWall)
        {
            // Lock the vertical position - set velocity to 0 to "stick"
            rb.velocity = new Vector2(rb.velocity.x, 0f);

            // OPTIONAL: You may want to prevent horizontal movement while stuck:
            // rb.velocity = new Vector2(0f, 0f); 

            // Prevent the animator from running its regular movement animations
            animator.enabled = false;
            isclimbing = false; // Ensure climbing flag is off

            // Display hands sprite while stuck (Optional - can be a different sprite)
            if (sprite != null && !isShowingHands)
            {
                sprite.enabled = true;
                if (mo != null)
                {
                    sprite.flipX = !mo.lookingright; // Flip hands to face wall
                }
            }
        }
        // ----------------------------------------------------
        // DEFAULT/NON-CLIMBING STATE
        // ----------------------------------------------------
        else
        {
            // If we are not climbing, and not currently stuck, ensure we are *un*-stuck
            if (isStuckToWall && !isTouchingWall)
            {
                // Auto-unstick if they move away from the wall while 'stuck' is still true
                ToggleWallStick(false);
            }

            isclimbing = false;

            if (animator != null && !isStuckToWall) // Only enable if we aren't stuck
            {
                animator.enabled = true;
            }
        }

    }

    private void ToggleWallStick(bool stick)
    {
        isStuckToWall = stick;

        // On unstick, re-enable physics/movement controls
        if (!stick)
        {
            if (sprite != null)
            {
                sprite.enabled = false;
            }
            if (animator != null)
            {
                animator.enabled = true;
            }
        }
    }

    private IEnumerator ShowHandsForDuration()
    {
        isShowingHands = true;
        if (sprite != null)
        {
            sprite.enabled = true;
        }
        yield return new WaitForSeconds(handDisplayDuration);

        // Only disable if we are not currently in the "stuck" state
        if (!isStuckToWall && sprite != null)
        {
            sprite.enabled = false;
        }
        isShowingHands = false;
    }

    public void SetWallTouch(bool touching)
    {
        isTouchingWall = touching;

        // IMPORTANT: If we lose touch with the wall while stuck, unstick immediately.
        if (!touching && isStuckToWall)
        {
            ToggleWallStick(false);
        }
    }
}