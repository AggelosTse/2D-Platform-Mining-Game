using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    [Header("Player Health")]
    public int health;

    [Header("Fall Damage Settings")]
    public float minFallDistance = 3f;    // Minimum height to take damage
    public float damageMultiplier = 10f; // Damage per unit of fall distance

    [Header("References")]
    public GameObject camer;
    private cameraShake cam;
    private Animator a;
    private bool isDead = false;

    public GameObject TileBreakerObj;
    TileBreaker tile;

    movement m;
    followPlayer f;
    wallClimb w;
    hittingSwoosh h;

    Rigidbody2D r;

    public GameObject enemy;
    public GameObject shop;
    ItemsScript shopscript;

    // Fall tracking
    private bool isFalling = false;
    private float fallStartY;

    // NEW: Flag to momentarily ignore fall damage after specific actions (like wall jump)
    private bool ignoreNextFallDamage = false;

    [HideInInspector] public bool isClimbing = false;

    void Start()
    {
        tile = TileBreakerObj.GetComponent<TileBreaker>();

        a = GetComponent<Animator>();
        w = GetComponent<wallClimb>();
        m = GetComponent<movement>();
        f = enemy.GetComponent<followPlayer>();
        h = GetComponent<hittingSwoosh>();
        r = GetComponent<Rigidbody2D>();

        cam = camer.GetComponent<cameraShake>();
        shopscript = shop.GetComponent<ItemsScript>();
    }

    void Update()
    {
        // Handle shop health purchase
        if (shopscript != null && shopscript.boughtHealth)
        {
            if (health <= 80)
            {
                health += 20;
                shopscript.boughtHealth = false;
            }
        }

        // Handle death
        if (health <= 0 && !isDead)
        {
            StartCoroutine(DeathSequence());
        }

        // -------------------------------------------------------------------
        // UPDATED: Check for climbing state to ignore fall damage
        // -------------------------------------------------------------------

        // Set the ignore flag if the player is in a wall interaction state
        if (w != null && (w.isclimbing /* Add isStuckToWall property if it exists in wallClimb */))
        {
            // If the player is actively climbing/sticking, reset fall tracking
            // and ensure the next landing ignores fall damage.
            if (!ignoreNextFallDamage)
            {
                ignoreNextFallDamage = true;
                ResetFallTracking();
            }
        }
        else if (m != null && m.isGrounded)
        {
            // Reset the flag upon grounding if no damage was applied
            ignoreNextFallDamage = false;
        }


        // Track falling
        if (m != null)
        {
            if (!m.isGrounded && r.velocity.y < 0 && !isFalling)
            {
                isFalling = true;
                fallStartY = transform.position.y;
            }

            if (m.isGrounded && isFalling)
            {
                float fallDistance = fallStartY - transform.position.y;
                isFalling = false; // Reset the falling state immediately

                // ONLY APPLY DAMAGE IF the fall distance is significant AND we are NOT ignoring it
                if (fallDistance > minFallDistance && !ignoreNextFallDamage)
                {
                    ApplyFallDamage(fallDistance);
                }

                // Always reset the ignore flag after a grounding event
                ignoreNextFallDamage = false;
            }
        }
    }

    IEnumerator DeathSequence()
    {
        isDead = true;

        // Camera shake
        if (cam != null) cam.StartShake(0.15f, 0.5f);

        // Trigger death animation
        if (a != null) a.SetBool("death", true);

        // Disable other scripts
        if (f != null) f.enabled = false;
        if (m != null) m.enabled = false;
        if (w != null) w.enabled = false;
        if (h != null) h.enabled = false;
        if (tile != null) tile.enabled = false;

        Collider2D col2D = GetComponent<Collider2D>();
        if (col2D != null)
        {
            if (col2D.sharedMaterial == null)
                col2D.sharedMaterial = new PhysicsMaterial2D();

            col2D.sharedMaterial.friction = 0f;
        }

        yield return new WaitForSeconds(2.5f);

        Destroy(gameObject);
        SceneManager.LoadScene(1);
    }

    public void ApplyFallDamage(float fallDistance)
    {
        // Damage is calculated based on the distance *over* the minimum threshold
        float damage = (fallDistance - minFallDistance) * damageMultiplier;
        health -= Mathf.RoundToInt(damage);
        Debug.Log($"Fall Damage: {damage}, Remaining Health: {health}");

        // Optional: camera shake on heavy fall
        if (cam != null && damage > 10f)
        {
            cam.StartShake(0.2f, 0.4f);
        }
    }

    public void ResetFallTracking()
    {
        isFalling = false;
        fallStartY = transform.position.y;
    }

}