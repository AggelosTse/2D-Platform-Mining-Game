using UnityEngine;

public class followPlayer : MonoBehaviour
{
    public string playerTag = "Player";
    public float moveSpeed = 3f;

    private Transform player;
    private Vector3 originalScale;
    private Rigidbody2D rb;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Player object with tag '" + playerTag + "' not found.");
        }

        originalScale = transform.localScale;
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate() // Use FixedUpdate for physics-based movement
    {
        if (player == null) return;

        if (Vector3.Distance(player.position, transform.position) < 4f)
        {
            Vector2 direction = (player.position - transform.position).normalized;

            // Move using Rigidbody2D
            Vector2 newPosition = rb.position + direction * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);

            // Flip sprite
            if (player.position.x < transform.position.x)
                transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
            else
                transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
    }
}
