using UnityEngine;

public class ladderClimbing : MonoBehaviour
{
    [Header("Climbing Settings")]
    public float climbSpeed = 4f;
    public float climbSmoothness = 5f;

    [Header("Ladder Detection")]
    public LayerMask ladderLayer; // set this to your Ladder layer in the inspector
    public float ladderCheckRadius = 0.2f;

    private Rigidbody2D rb;
    private wallClimb wall;
    private Health health; // <-- reference to your Health script
    private bool isClimbing;
    private float verticalInput;
    private float defaultGravity;

    private void Start()
    {
        wall = GetComponent<wallClimb>();
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>(); // get Health component
        defaultGravity = rb.gravityScale;
    }

    private void Update()
    {
        // If inside ladder trigger, allow vertical input
        verticalInput = isClimbing ? Input.GetAxisRaw("Vertical") : 0f;

        // Safety net: stop climbing if no ladder is detected under the player
        if (isClimbing && !IsTouchingLadder())
        {
            Debug.Log("No ladder detected, stopping climb.");
            StopClimbing();
        }
    }

    private void FixedUpdate()
    {
        if (isClimbing)
        {
            rb.gravityScale = 0f; // disable gravity while climbing

            float targetY = verticalInput * climbSpeed;
            Vector2 targetVelocity = new Vector2(rb.velocity.x, targetY);

            // Smooth climb movement
            rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, climbSmoothness * Time.fixedDeltaTime);
        }
        else
        {
            rb.gravityScale = defaultGravity; // restore gravity when not climbing
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            Debug.Log("Entered ladder trigger: " + collision.name);
            StartClimbing();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            // This keeps you in climbing state even if Enter was missed
            if (!isClimbing)
            {
                Debug.Log("Staying in ladder trigger: " + collision.name);
                StartClimbing();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            Debug.Log("Exited ladder trigger: " + collision.name);
            StopClimbing();
        }
    }

    private void StartClimbing()
    {
        isClimbing = true;
        wall.enabled = false;
        rb.velocity = new Vector2(rb.velocity.x, 0f); // stop falling instantly

        if (health != null)
        {
            health.isClimbing = true;        // <-- tell health we are climbing
            health.ResetFallTracking();      // <-- custom method to clear fallStartY
        }
    }

    private void StopClimbing()
    {
        isClimbing = false;
        wall.enabled = true;

        if (health != null)
        {
            health.isClimbing = false; // re-enable fall tracking
        }
    }

    private bool IsTouchingLadder()
    {
        Collider2D ladderCheck = Physics2D.OverlapCircle(transform.position, ladderCheckRadius, ladderLayer);
        return ladderCheck != null;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw ladder detection radius in editor for debugging
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, ladderCheckRadius);
    }
}
