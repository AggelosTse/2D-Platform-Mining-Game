using UnityEngine;

public class combat : MonoBehaviour
{
    public float attackRange = 1f;                  // Radius of melee attack
    public LayerMask enemyLayers;                   // Layer mask for enemies
    public float pushForce = 5f;                    // Knockback force

    private hittingSwoosh swoosh;                   // Reference to the swoosh animation
    movement m;

    GameObject hit;
    SpriteRenderer hitsp;
    private void Start()
    {

        m = GetComponent<movement>();
        swoosh = GetComponent<hittingSwoosh>();     // Get the swoosh component
        hit = swoosh.hitanim;
        hitsp = hit.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))            // Left mouse click
        {
            Attack();
        }
    }

    void Attack()
    {
        Vector2 facingDirection = m.lookingright ? Vector2.right : Vector2.left;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            Vector2 toEnemy = (enemy.transform.position - transform.position).normalized;

            float verticalTolerance = 0.7f;
            float verticalComponent = Mathf.Abs(toEnemy.y);

            if (verticalComponent < verticalTolerance)
            {
                float dot = Vector2.Dot(facingDirection, toEnemy);
                if (dot <= 0f)
                    continue; // enemy behind player, skip
            }

            if (swoosh != null)
            {
                float rotAngle = Mathf.Atan2(toEnemy.y, toEnemy.x) * Mathf.Rad2Deg;
                float rotationZ = GetRotationFromDirection(rotAngle);
                swoosh.Play(rotationZ);
            }

            enemyhealth enemyHealthComponent = enemy.GetComponent<enemyhealth>();
            if (enemyHealthComponent != null)
            {
                enemyHealthComponent.TakeDamage();

                Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    Vector2 pushDirection = toEnemy;
                    if (pushDirection == Vector2.zero)
                        pushDirection = Vector2.up;

                    pushDirection += Vector2.up * 0.5f;
                    pushDirection.Normalize();

                    enemyRb.AddForce(pushDirection * pushForce, ForceMode2D.Impulse);
                }
            }

            break; // hit only one enemy
        }
    }



    float GetRotationFromDirection(float angle)
    {
        // Normalize angle to [0, 360)
        angle = (angle + 360) % 360;

        if (angle >= 45f && angle < 135f)
        {
            hitsp.flipY = true;
            return 180f;      // Up
        }
        
          
        else if (angle >= 135f && angle < 225f)
            return 90f;     // Left
        else if (angle >= 225f && angle < 315f)
            return -180f;     // Down
        else
            return -90f;       // Right
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
