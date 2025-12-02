using System.Collections;
using UnityEngine;

public class UsingRope : MonoBehaviour
{
    public GameObject shop;
    ItemsScript shopScript;

    [Header("Rope Prefabs")]
    public GameObject ropeStartPrefab;
    public GameObject ropeSegmentPrefab;

    [Header("Settings")]
    public Transform ropeOrigin;
    public GameObject caveTilemap;      // <-- Reference your Tilemap GameObject here
    public float spawnDelay = 0.05f;

    private bool isSpawning = false;
    private float segmentLength;
    private int caveLayer;              // We'll grab this from the Tilemap automatically

    void Start()
    {
        shopScript = shop.GetComponent<ItemsScript>();

        // Get cave layer from the tilemap
        if (caveTilemap != null)
        {
            caveLayer = caveTilemap.layer;
        }
        else
        {
            Debug.LogError("Cave Tilemap not assigned! Rope will not collide correctly.");
        }

        // Determine rope segment spacing from prefab's collider
        BoxCollider2D col = ropeSegmentPrefab.GetComponent<BoxCollider2D>();
        if (col != null)
        {
            segmentLength = col.size.y * ropeSegmentPrefab.transform.localScale.y;
        }
        else
        {
            segmentLength = 0.5f; // fallback
            Debug.LogWarning("RopeSegmentPrefab has no BoxCollider2D, using default segmentLength=0.5");
        }
    }

    void Update()
    {
        if (shopScript.boughtRope)
        {
            if (Input.GetKeyDown(KeyCode.E) && !isSpawning && shopScript.ropesPurchased > 0)
            {
                StartCoroutine(SpawnRope());
            }
        }
    }

    private IEnumerator SpawnRope()
    {
        isSpawning = true;

        float facingDirection = transform.localScale.x >= 0 ? 1f : -1f;
        Vector2 basePosition = ropeOrigin.position + new Vector3(facingDirection * 1f, 0.5f, 0f);

        BoxCollider2D prefabCollider = ropeSegmentPrefab.GetComponent<BoxCollider2D>();
        Vector2 colliderSize = prefabCollider.size * ropeSegmentPrefab.transform.localScale;

        // --- Check if space near player is empty ---
        Collider2D startOverlap = Physics2D.OverlapBox(basePosition, colliderSize, 0f, 1 << caveLayer);
        if (startOverlap != null)
        {
            Debug.Log("No space to place rope near the player.");
            isSpawning = false;
            yield break; // stop if blocked
        }

        // --- Check how many segments can fit downward ---
        int maxPlaceableSegments = 0;
        Vector2 checkPos = basePosition;

        while (true)
        {
            RaycastHit2D hit = Physics2D.BoxCast(checkPos, colliderSize, 0f, Vector2.down, segmentLength, 1 << caveLayer);
            if (hit.collider != null)
                break;

            checkPos += Vector2.down * segmentLength;
            maxPlaceableSegments++;
        }

        // If fewer than 3 segments would fit, don't spawn rope at all
        if (maxPlaceableSegments < 3)
        {
            Debug.Log("Not enough space to spawn a rope.");
            isSpawning = false;
            yield break;
        }

        // --- Spawn rope ---
        Vector2 currentPosition = basePosition;

        // Spawn first rope segment (anchor)
        Instantiate(ropeStartPrefab, currentPosition, Quaternion.identity);

        for (int i = 0; i < maxPlaceableSegments; i++)
        {
            currentPosition += Vector2.down * segmentLength;
            Instantiate(ropeSegmentPrefab, currentPosition, Quaternion.identity);
            yield return new WaitForSeconds(spawnDelay);
        }

        // Deduct rope from inventory
        shopScript.ropesPurchased -= 1;
        if (shopScript.ropesPurchased <= 0)
            shopScript.boughtRope = false;

        isSpawning = false;
    }



    private Vector2 FindNearestValidPosition(Vector2 basePos, Vector2 colliderSize)
    {
        float searchRange = 1f;
        float step = 0.1f;

        float bestDistance = float.MaxValue;
        Vector2 bestPosition = Vector2.negativeInfinity;

        for (float offset = -searchRange; offset <= searchRange; offset += step)
        {
            Vector2 candidate = basePos + Vector2.up * offset;
            Collider2D overlap = Physics2D.OverlapBox(candidate, colliderSize, 0f, 1 << caveLayer);

            if (overlap == null)
            {
                float distance = Mathf.Abs(offset);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestPosition = candidate;
                }
            }
        }

        return bestPosition;
    }





    private void OnDrawGizmosSelected()
    {
        if (ropeSegmentPrefab != null && ropeOrigin != null)
        {
            BoxCollider2D prefabCollider = ropeSegmentPrefab.GetComponent<BoxCollider2D>();
            if (prefabCollider != null)
            {
                Vector2 colliderSize = prefabCollider.size * ropeSegmentPrefab.transform.localScale;
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(ropeOrigin.position + Vector3.down * colliderSize.y * 0.5f, colliderSize);
            }
        }
    }
}
