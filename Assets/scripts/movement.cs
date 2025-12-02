using UnityEngine;
using System.Collections;

public class movement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;


    private Rigidbody2D rb;
    public bool isGrounded;
    private Animator anim;
    private SpriteRenderer sprite;

    public bool lookingright;
    private bool run;
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        lookingright = true;
        run = false;
    }

    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput != 0)
        {
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
            run = true;
        }
        else
        {
            // Decelerate manually
            float newXVelocity = rb.velocity.x * (1 - Time.deltaTime * 10f); // 10 = deceleration rate
            if (Mathf.Abs(newXVelocity) < 0.05f) newXVelocity = 0f;
            rb.velocity = new Vector2(newXVelocity, rb.velocity.y);
            run = false;
        }


        if (moveInput < 0)
        {
            sprite.flipX = true;
            lookingright = false;
        }
        else if (moveInput > 0)
        {
            sprite.flipX = false;
            lookingright = true;
        }

        anim.SetBool("isrunning", moveInput != 0);

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            StartCoroutine(PlayJumpAnimation());
        }
    }

    IEnumerator PlayJumpAnimation()
    {
        anim.SetBool("isjumping", true);

        if (run)
        {
            anim.SetBool("isrunning", false);
        }
        yield return new WaitForSeconds(0.2f);


        run = !run;
        anim.SetBool("isjumping", false);
    }
}
