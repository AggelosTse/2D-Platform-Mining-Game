using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using System.Collections.Generic;

public class TileBreaker : MonoBehaviour
{
    bogdanosDialogue dialogue;
    public GameObject blud;

    shopGuyDialogues shopGuy;
    public GameObject shopg;

    public Tilemap tilemap;
    public Transform player;
    public float maxBreakDistance = 2f;

    public caveTest caveTestReference; // Reference to caveTest to get ore info




    private Dictionary<Vector3Int, int> tileHitPoints = new Dictionary<Vector3Int, int>();
    private Dictionary<Vector3Int, GameObject> activeCracks = new Dictionary<Vector3Int, GameObject>();

    [Header("Crack Prefabs (in order of severity)")]
    public List<GameObject> crackPrefabs;

    [Header("Ore Break Settings")]
    public int orePiecesMin = 2;
    public int orePiecesMax = 3;
    public float orePieceForce = 3f;
    public float orePieceUpwardBias = 1.2f;

    [Header("Ore Prefabs Per Type")]
    public List<GameObject> goldOrePrefabs = new List<GameObject>();
    public List<GameObject> ironOrePrefabs = new List<GameObject>();
    public List<GameObject> purpleOrePrefabs = new List<GameObject>();
    public List<GameObject> greenOrePrefabs = new List<GameObject>();

    [Header("Ore Ingots Per Type")]
    public GameObject ironIngot;
    public GameObject greenIngot;
    public GameObject PurpleIngot;
    public GameObject GoldIngot;

    private readonly Vector3Int[] cardinalDirections = new Vector3Int[]
    {
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.right,
        Vector3Int.left
    };

    SpriteRenderer s;
    public GameObject hit;

    public GameObject shop;
    ItemsScript shopScript;

    wallClimb climbScript;

    public float Hitcooldown;
    public float MaxCooldownNeeded;

    private Vector3Int playerFacing = Vector3Int.right;

    void Start()
    {
        dialogue = blud.GetComponent<bogdanosDialogue>();
        shopGuy = shopg.GetComponent<shopGuyDialogues>();

        shopScript = shop.GetComponent<ItemsScript>();
        climbScript = player.GetComponent<wallClimb>();
        Hitcooldown = 0;


    }

    void Update()
    {
        Hitcooldown += Time.deltaTime;

        if (climbScript != null && climbScript.isclimbing)
            return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(h) > 0.1f)
            playerFacing = h > 0 ? Vector3Int.right : Vector3Int.left;
        else if (Mathf.Abs(v) > 0.1f)
            playerFacing = v > 0 ? Vector3Int.up : Vector3Int.down;



        if (!Input.GetMouseButtonDown(0) || Hitcooldown < MaxCooldownNeeded || !dialogue.firstDialgueFinishedBlud || !shopGuy.firstDialogueFinished) 
            return;



        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = -Camera.main.transform.position.z;
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        Vector3Int clickedCell = tilemap.WorldToCell(worldMousePos);
        Vector3 tileCenterWorld = tilemap.GetCellCenterWorld(clickedCell);

        if (!tilemap.HasTile(clickedCell))
            return;

        Vector3Int playerTile;
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        playerTile = playerCollider != null ? tilemap.WorldToCell(playerCollider.bounds.center) :
                                              tilemap.WorldToCell(player.position + Vector3.down * 0.1f);

        bool isAdjacent = false;
        Vector3Int direction = Vector3Int.zero;

        foreach (Vector3Int dir in cardinalDirections)
        {
            if (clickedCell == playerTile + dir && dir == playerFacing)
            {
                isAdjacent = true;
                direction = dir;
                break;
            }
        }

        if (!isAdjacent) return;

        // Play swoosh
        hittingSwoosh swoosh = FindObjectOfType<hittingSwoosh>();
        s = hit.GetComponent<SpriteRenderer>();
        if (swoosh != null)
        {
            float angle = 0f;
            if (direction == Vector3Int.right) { s.flipY = false; angle = -90f; }
            else if (direction == Vector3Int.up) { s.flipY = true; angle = -180f; }
            else if (direction == Vector3Int.left) { s.flipY = false; angle = 90f; }
            else if (direction == Vector3Int.down) { s.flipY = false; angle = 180f; }
            swoosh.Play(angle);
        }

        // Tile health
        if (!tileHitPoints.ContainsKey(clickedCell))
            tileHitPoints[clickedCell] = shopScript.boughtHand ? 2 : Random.Range(3, 4);

        tileHitPoints[clickedCell]--;

        Hitcooldown = 0;

        int remainingHealth = tileHitPoints[clickedCell];
        ShowCrack(clickedCell, tileCenterWorld, remainingHealth);

        if (remainingHealth > 0) return;

        TileBase tileBroken = tilemap.GetTile(clickedCell);
        OreTileType brokenOre = null;

        if (caveTestReference != null && tileBroken != null)
        {
            foreach (var oreType in caveTestReference.ores)
            {
                if (oreType.oreTile == tileBroken)
                {
                    brokenOre = oreType;
                    break;
                }
            }
        }

        // Coins and prefabs per ore
        
        List<GameObject> prefabSet = null;

        if (brokenOre != null)
        {
            string oreName = brokenOre.oreTile.name;

            if (oreName.Contains("Gold"))
            {
                
                prefabSet = goldOrePrefabs;
                Vector3 spawnPos = tilemap.GetCellCenterWorld(clickedCell);

                for (int i = 0; i < 3; i++)
                {
                    Vector3 randomOffset = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0f);
                    GameObject ingot = Instantiate(GoldIngot, spawnPos + randomOffset, Quaternion.identity);
                }
            }
            else if (oreName.Contains("Iron"))
            {
                
                prefabSet = ironOrePrefabs;
                Vector3 spawnPos = tilemap.GetCellCenterWorld(clickedCell);

                for (int i = 0; i < 3; i++)
                {
                    Vector3 randomOffset = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0f);
                    GameObject ingot = Instantiate(ironIngot, spawnPos + randomOffset, Quaternion.identity);
                }
            }
            else if (oreName.Contains("Purple"))
            {
               
                Vector3 spawnPos = tilemap.GetCellCenterWorld(clickedCell);

                for (int i = 0; i < 3; i++)
                {
                    Vector3 randomOffset = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0f);
                    GameObject ingot = Instantiate(PurpleIngot, spawnPos + randomOffset, Quaternion.identity);
                }
                prefabSet = purpleOrePrefabs;
            }
            else if (oreName.Contains("Green"))
            {
               
                prefabSet = greenOrePrefabs;
                Vector3 spawnPos = tilemap.GetCellCenterWorld(clickedCell);

                for (int i = 0; i < 3; i++)
                {
                    Vector3 randomOffset = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0f);
                    GameObject ingot = Instantiate(greenIngot, spawnPos + randomOffset, Quaternion.identity);
                }
            }

            if (prefabSet != null && prefabSet.Count > 0)
                SpawnOrePieces(tileCenterWorld, prefabSet);
        }

       
       

        tilemap.SetTile(clickedCell, null);

        if (activeCracks.ContainsKey(clickedCell))
        {
            Destroy(activeCracks[clickedCell]);
            activeCracks.Remove(clickedCell);
        }

        // Remove flower above
        Vector3Int aboveTilePos = new Vector3Int(clickedCell.x, clickedCell.y + 1, clickedCell.z);
        TileBase aboveTile = tilemap.GetTile(aboveTilePos);
        if (aboveTile != null && caveTestReference != null)
        {
            foreach (var flower in caveTestReference.flowers)
            {
                if (flower.flowerTile == aboveTile)
                {
                    tilemap.SetTile(aboveTilePos, null);
                    break;
                }
            }
        }
        
    }





    void ShowCrack(Vector3Int cellPos, Vector3 worldPos, int remainingHealth)
    {
        if (activeCracks.ContainsKey(cellPos))
        {
            Destroy(activeCracks[cellPos]);
            activeCracks.Remove(cellPos);
        }

        if (crackPrefabs == null || crackPrefabs.Count == 0) return;

        int maxHealth = 3;
        int crackIndex = Mathf.Clamp(maxHealth - remainingHealth - 1, 0, crackPrefabs.Count - 1);

        GameObject newCrack = Instantiate(crackPrefabs[crackIndex], worldPos, Quaternion.identity);
        newCrack.transform.SetParent(tilemap.transform);
        activeCracks[cellPos] = newCrack;
    }

    void SpawnOrePieces(Vector3 spawnPosition, List<GameObject> prefabSet)
    {
        int count = Random.Range(orePiecesMin, orePiecesMax + 1);

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = prefabSet[Random.Range(0, prefabSet.Count)];
            GameObject piece = Instantiate(prefab, spawnPosition, Quaternion.identity);

            Rigidbody2D rb = piece.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 randomDir = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, orePieceUpwardBias)).normalized;
                rb.AddForce(randomDir * orePieceForce, ForceMode2D.Impulse);
            }
        }
    }
}
