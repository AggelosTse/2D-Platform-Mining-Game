using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class caveTest : MonoBehaviour
{
    [Header("Settings")]
    public int width = 50;
    public int height = 30;
    [Range(0f, 1f)] public float fillPercent = 0.6f;
    public float noiseScale = 0.1f;
    [Tooltip("The vertical distance from the top of the chunk where caves will start generating.")]
    public int caveSpawnBelowTop = 10;

    [Header("Variation")]
    [Range(0f, 1f)] public float fillVariationStrength = 0.3f;
    public float fillVariationScale = 0.05f;

    public Vector2 offset = Vector2.zero;
    public bool relativeToPlayer = true;
    public Transform player;

    [Header("Tiles")]
    public Tilemap tilemap;
    public List<TileBase> grassTiles;
    public List<TileBase> dirtTiles;
    public List<TileBase> stoneTiles;
    public List<SpecialPrefabType> specialPrefabs;
    public List<OreTileType> ores;
    public List<FlowerTileType> flowers;
    public List<CrystaliteTileType> crystalites;
    public List<TopperTileType> toppers;
    public List<RoofTileType> roofs;
    public List<VaneTileType> vanes;

    [Header("Dungeon Generation")]
    [Tooltip("Chance (0-1) to place a block inside the empty cavern space, creating internal terrain.")]
    [Range(0f, 0.5f)] public float dungeonInternalFillChance = 0.3f;
    [Range(0f, 1f)] public float dungeonFlowerSpawnChance = 0.15f;
    [Range(0f, 1f)] public float dungeonVaneSpawnChance = 0.12f;

    [Header("Dungeon Structure")]
    [Tooltip("Prefab to place on top of the 1x3 block pillar inside the dungeon.")]
    public GameObject fixedDungeonPrefab;
    [Range(0f, 1f)]
    [Tooltip("Chance to spawn the fixed 1x3 structure. Set to 1 for always.")]
    public float fixedStructureSpawnChance = 1f;

    [Header("Dungeon Determinism")]
    [Tooltip("The fixed integer seed used only for the dungeon's shape and internal feature placement.")]
    public int dungeonSeed = 1337; // A fixed seed for repeatability

    [Header("Dirt Layer")]
    public int minDirtDepth = 5;
    public int maxDirtDepth = 9;

    [Header("Crystalite Settings")]
    public GameObject fallingCrystalitePrefab;
    public float detectionRangeX = 1.5f;
    public float detectionRangeY = 4f;
    [Range(0f, 1f)] public float fallChanceNearPlayer = 0.01f;

    [Header("Enemies")]
    public List<GameObject> enemyPrefabs;
    [Range(0f, 1f)] public float enemySpawnChance = 0.02f;
    [Tooltip("Distance from the top of the tilemap where enemies will start to spawn.")]
    public int enemySpawnBelowTop = 50;

    private bool[,] caveMap;
    private List<Vector3Int> crystalitePositions = new List<Vector3Int>();

    private TileBase[] _stoneTiles;
    private TileBase[] _dirtTiles;
    private TileBase[] _grassTiles;
    private OreTileType[] _oreTypes;
    private FlowerTileType[] _flowerTypes;
    private CrystaliteTileType[] _crystaliteTypes;
    private TopperTileType[] _topperTypes;
    private RoofTileType[] _roofTypes;
    private VaneTileType[] _vaneTypes;
    private HashSet<TileBase> stoneSet;

    private float mainSeed;

    void Awake()
    {
        _stoneTiles = stoneTiles.ToArray();
        _dirtTiles = dirtTiles.ToArray();
        _grassTiles = grassTiles.ToArray();
        _oreTypes = ores.ToArray();
        _flowerTypes = flowers.ToArray();
        _crystaliteTypes = crystalites.ToArray();
        _topperTypes = toppers.ToArray();
        _roofTypes = roofs.ToArray();
        _vaneTypes = vanes.ToArray();
        stoneSet = new HashSet<TileBase>(_stoneTiles);
    }

    void Start()
    {
        // Use a random seed for the main cave generation (outside the dungeon)
        mainSeed = Random.Range(-10000f, 10000f);
        GenerateCave();
    }

    void Update()
    {
        CheckCrystaliteSupport();
        CheckCrystaliteNearPlayer();
    }

    void GenerateCave()
    {
        caveMap = new bool[width, height];
        crystalitePositions.Clear();
        tilemap.ClearAllTiles();

        Vector2 origin = relativeToPlayer && player != null
            ? (Vector2)player.position + offset - new Vector2(width / 2f, height + 2f)
            : offset;

        int ox = Mathf.FloorToInt(origin.x);
        int oy = Mathf.FloorToInt(origin.y);

        // Define Dungeon Area Boundaries
        const int dungeonHeight = 8;
        int dungeonYStart = 1;
        int dungeonYEnd = dungeonYStart + dungeonHeight;

        // Create a *seeded* Random object for deterministic dungeon internal placement/checks
        // Note: This does NOT affect PerlinNoise (which uses its own float seed) or Unity's Random (used for the rest of the world)
        System.Random dungeonRandom = new System.Random(dungeonSeed);

        // Step 1: Generate base cave map
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (y >= dungeonYStart && y < dungeonYEnd)
                {
                    // Mark as solid for now, will be carved later
                    caveMap[x, y] = true;
                }
                else
                {
                    bool edge = x == 0 || y == 0 || x == width - 1 || y == height - 1;

                    if ((height - 1) - y < caveSpawnBelowTop)
                    {
                        caveMap[x, y] = true;
                    }
                    else
                    {
                        // Use the mainSeed for the rest of the world
                        float baseNoise = Mathf.PerlinNoise((x + mainSeed) * noiseScale, (y + mainSeed) * noiseScale);
                        float localFillMod = Mathf.PerlinNoise((x + mainSeed + 5000f) * fillVariationScale, (y + mainSeed + 5000f) * fillVariationScale);
                        float localFill = Mathf.Clamp01(fillPercent + (localFillMod - 0.5f) * 2f * fillVariationStrength);
                        caveMap[x, y] = edge || baseNoise > 1f - localFill;
                    }
                }
            }
        }

        // CarveHoles uses Unity's Random.Range, which varies with the main seed
        CarveHoles(3, 7, 3, 5, 4);

        int dirtLayerEnd = Random.Range(minDirtDepth, maxDirtDepth + 1);

        // Step 2: Fill tiles and place special prefabs (outside dungeon zone)
        for (int y = 0; y < height; y++)
        {
            int worldY = oy + y;
            bool belowDepth = (height - 1) - y >= caveSpawnBelowTop;
            bool isInDungeonExclusionZone = y >= dungeonYStart && y < dungeonYEnd;

            for (int x = 0; x < width; x++)
            {
                int worldX = ox + x;

                if (caveMap[x, y])
                {
                    TileBase tile = null;

                    if (y == height - 1 && _grassTiles.Length > 0)
                        tile = _grassTiles[Random.Range(0, _grassTiles.Length)];
                    else if (y >= height - dirtLayerEnd && y < height - 1 && _dirtTiles.Length > 0)
                        tile = _dirtTiles[Random.Range(0, _dirtTiles.Length)];
                    else
                    {
                        tile = GetRandomStoneTile();
                        // Ore/Mineral exclusion - uses Unity's Random.value, which varies
                        if (belowDepth && !isInDungeonExclusionZone)
                        {
                            foreach (var ore in _oreTypes)
                            {
                                if ((height - 1) - y >= ore.spawnBelowTop && Random.value < ore.spawnChance)
                                {
                                    tile = ore.oreTile;
                                    break;
                                }
                            }
                        }
                    }
                    tilemap.SetTile(new Vector3Int(worldX, worldY, 0), tile);
                }
                // Special Prefab exclusion - uses Unity's Random.value, which varies
                else if (!caveMap[x, y] && y > 0 && !isInDungeonExclusionZone)
                {
                    foreach (var special in specialPrefabs)
                    {
                        if ((height - 1) - y >= special.spawnBelowTop && Random.value < special.spawnChance)
                        {
                            int run = Random.Range(3, 7);
                            if (x + run >= width) run = width - x - 1;
                            bool canPlace = true;
                            // ... (Special Prefab placement logic remains the same) ...
                            for (int i = 0; i < run; i++)
                            {
                                int rx = x + i;
                                if (caveMap[rx, y] || tilemap.GetTile(new Vector3Int(ox + rx, oy + y - 1, 0)) == null)
                                {
                                    canPlace = false;
                                    break;
                                }
                            }

                            if (canPlace)
                            {
                                for (int i = 0; i < run; i++)
                                {
                                    int rx = x + i;
                                    Vector3 worldPos = new Vector3(ox + rx + 0.5f, worldY + 0.5f, 0f);
                                    Instantiate(special.specialPrefab, worldPos, Quaternion.identity);
                                    // Mark as solid to prevent other things from spawning on top
                                    caveMap[rx, y] = true;
                                }
                                x += run - 1;
                            }
                        }
                    }
                }
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Step 3: Carve, Smooth, and Fill Dungeon Area (Must be deterministic)
        // --------------------------------------------------------------------------------------------------

        // 3a. Initial Carve (Curvy walls) - Uses fixed dungeonSeed for noise
        float dungeonNoiseSeed = (float)dungeonSeed; // Use the fixed integer seed as a float offset for Perlin noise
        int centerX = width / 2;
        int centerY = (dungeonYStart + dungeonYEnd) / 2;
        float maxRadiusSquared = Mathf.Pow(width / 2f - 3f, 2f);

        bool[,] carveMapBuffer = new bool[width, height];

        for (int y = dungeonYStart; y < dungeonYEnd; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int worldX = ox + x;
                int worldY = oy + y;
                Vector3Int cellPos = new Vector3Int(worldX, worldY, 0);

                bool isPerimeter = x < 2 || x >= width - 2 || y == dungeonYStart || y == dungeonYEnd - 1;

                if (isPerimeter)
                {
                    tilemap.SetTile(cellPos, GetRandomStoneTile());
                    carveMapBuffer[x, y] = true;
                }
                else
                {
                    float distSq = Mathf.Pow(x - centerX, 2) + Mathf.Pow(y - centerY, 2);

                    if (distSq < maxRadiusSquared)
                    {
                        // Use the fixed dungeonNoiseSeed here
                        float noise = Mathf.PerlinNoise((x + dungeonNoiseSeed) * 0.3f, (y + dungeonNoiseSeed) * 0.3f) * 0.6f;
                        float normalizedDist = distSq / maxRadiusSquared;

                        if (normalizedDist < 0.5f + noise)
                        {
                            tilemap.SetTile(cellPos, null);
                            carveMapBuffer[x, y] = false; // Empty
                        }
                        else
                        {
                            tilemap.SetTile(cellPos, GetRandomStoneTile());
                            carveMapBuffer[x, y] = true; // Solid
                        }
                    }
                    else
                    {
                        tilemap.SetTile(cellPos, GetRandomStoneTile());
                        carveMapBuffer[x, y] = true;
                    }
                }
            }
        }

        // 3b. Smoothing Pass (deterministic as it only uses map data)
        for (int i = 0; i < 2; i++)
        {
            for (int y = dungeonYStart + 1; y < dungeonYEnd - 1; y++)
            {
                for (int x = 2; x < width - 2; x++)
                {
                    int solidNeighborCount = GetSolidNeighborCount(carveMapBuffer, x, y);

                    if (carveMapBuffer[x, y] == false && solidNeighborCount >= 5)
                    {
                        carveMapBuffer[x, y] = true;
                        tilemap.SetTile(new Vector3Int(ox + x, oy + y, 0), GetRandomStoneTile());
                    }
                    else if (carveMapBuffer[x, y] == true && solidNeighborCount <= 4)
                    {
                        carveMapBuffer[x, y] = false;
                        tilemap.SetTile(new Vector3Int(ox + x, oy + y, 0), null);
                    }
                }
            }
        }

        // 3c. Place Fixed Dungeon Structure (1x4 pillar + Prefab) - Uses seeded System.Random
        if (fixedDungeonPrefab != null && dungeonRandom.NextDouble() < fixedStructureSpawnChance)
        {
            // Note: PlaceFixedStructure is modified to accept and use the dungeonRandom object.
            PlaceFixedStructure(ox, oy, dungeonYStart, dungeonYEnd, carveMapBuffer, dungeonRandom);
        }

        // 3d. Internal Fill (Sparse, walkable blocks) - Uses seeded System.Random
        for (int y = dungeonYStart + 1; y < dungeonYEnd - 1; y++)
        {
            for (int x = 2; x < width - 2; x++)
            {
                // Only fill if the cell is currently empty AND if the cell below is solid 
                if (carveMapBuffer[x, y] == false && carveMapBuffer[x, y - 1] == true)
                {
                    if (dungeonRandom.NextDouble() < dungeonInternalFillChance)
                    {
                        carveMapBuffer[x, y] = true;
                        tilemap.SetTile(new Vector3Int(ox + x, oy + y, 0), GetRandomStoneTile());
                    }
                }
            }
        }

        // Finalize: Copy the result back to the main caveMap
        for (int y = dungeonYStart; y < dungeonYEnd; y++)
        {
            for (int x = 0; x < width; x++)
            {
                caveMap[x, y] = carveMapBuffer[x, y];
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Step 4: Place other elements 
        // --------------------------------------------------------------------------------------------------
        // Note: These now use the dungeonRandom object if placing features *inside* the dungeon zone.
        PlaceFlowers(ox, oy, dungeonYStart, dungeonYEnd, dungeonRandom);
        PlaceVanes(ox, oy, dungeonYStart, dungeonYEnd, dungeonRandom);
        PlaceCrystalites(ox, oy, dungeonYStart, dungeonYEnd); // Crystalites outside dungeon only
        PlaceEnemies(ox, oy, dungeonYStart, dungeonYEnd); // Enemies outside dungeon only
        PlaceToppers(ox, oy, dungeonYStart, dungeonYEnd); // Toppers outside dungeon only
        PlaceRoofs(ox, oy, dungeonYStart, dungeonYEnd); // Roofs outside dungeon only
    }

    // ---

    // MODIFIED STRUCTURE PLACEMENT METHOD: Now accepts and uses the seeded System.Random
    void PlaceFixedStructure(int ox, int oy, int dungeonYStart, int dungeonYEnd, bool[,] carveMapBuffer, System.Random dungeonRandom)
    {
        const int structureW = 1;
        const int structureH = 3;
        const int totalStructureHeight = structureH + 1;

        List<Vector2Int> potentialSpots = new List<Vector2Int>();

        for (int y = dungeonYStart + 1; y < dungeonYEnd - totalStructureHeight; y++)
        {
            for (int x = 2; x < width - 2 - structureW; x++)
            {
                bool isClear = true;

                // 1. Check if the entire 1x4 area is clear
                for (int sy = y; sy < y + totalStructureHeight; sy++)
                {
                    if (carveMapBuffer[x, sy] == true)
                    {
                        isClear = false;
                        break;
                    }
                }

                // 2. Check for solid foundation below (y - 1)
                if (isClear)
                {
                    bool hasFoundation = carveMapBuffer[x, y - 1] == true;
                    if (hasFoundation)
                    {
                        potentialSpots.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        if (potentialSpots.Count > 0)
        {
            // Use the seeded Random object for selecting the spawn point
            Vector2Int spawnSpot = potentialSpots[dungeonRandom.Next(0, potentialSpots.Count)];
            int sx = spawnSpot.x;
            int sy = spawnSpot.y;

            // 1. Place 1x4 solid stone blocks
            for (int y = sy; y < sy + totalStructureHeight; y++)
            {
                Vector3Int cellPos = new Vector3Int(ox + sx, oy + y, 0);
                tilemap.SetTile(cellPos, GetRandomStoneTile());
                carveMapBuffer[sx, y] = true;
            }

            // 2. Instantiate the prefab on top of the blocks.
            Vector3Int prefabCell = new Vector3Int(ox + sx, oy + sy + totalStructureHeight, 0);
            Vector3 cellBottomLeft = tilemap.CellToWorld(prefabCell);
            Vector3 worldPos = cellBottomLeft + new Vector3(0.5f, 0f, 0f);

            Instantiate(fixedDungeonPrefab, worldPos, Quaternion.identity);
        }
    }

    // ---
    // HELPER METHODS
    // ---

    private int GetSolidNeighborCount(bool[,] map, int gridX, int gridY)
    {
        int solidCount = 0;
        for (int neighborX = gridX - 1; neighborX <= gridX + 1; neighborX++)
        {
            for (int neighborY = gridY - 1; neighborY <= gridY + 1; neighborY++)
            {
                if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                {
                    if (neighborX != gridX || neighborY != gridY)
                    {
                        if (map[neighborX, neighborY])
                        {
                            solidCount++;
                        }
                    }
                }
            }
        }
        return solidCount;
    }

    TileBase GetRandomStoneTile()
    {
        if (_stoneTiles.Length == 0) return null;
        return _stoneTiles[Random.Range(0, _stoneTiles.Length)];
    }

    bool IsCrystalite(TileBase tile)
    {
        foreach (var crystal in _crystaliteTypes)
            if (crystal.crystaliteTile == tile) return true;
        return false;
    }

    bool IsRoof(TileBase tile)
    {
        foreach (var roof in _roofTypes)
            if (roof.roofTile == tile) return true;
        return false;
    }

    bool IsStoneOrGroundTile(TileBase tile)
    {
        return stoneSet.Contains(tile) ||
                System.Array.Exists(_dirtTiles, t => t == tile) ||
                System.Array.Exists(_grassTiles, t => t == tile) ||
                System.Array.Exists(_oreTypes, o => o.oreTile == tile);
    }

    void CarveHoles(int minW, int maxW, int minH, int maxH, int count)
    {
        for (int i = 0; i < count; i++)
        {
            int w = Random.Range(minW, maxW + 1);
            int h = Random.Range(minH, maxH + 1);
            int x = Random.Range(1, width - w - 1);
            int y = Random.Range(1, height - h - 1);

            for (int yy = y; yy < y + h; yy++)
                for (int xx = x; xx < x + w; xx++)
                    caveMap[xx, yy] = false;
        }
    }

    // ---
    // PLACEMENT METHODS 
    // ---

    // MODIFIED: Now accepts and uses the seeded System.Random for placement inside the dungeon zone.
    void PlaceFlowers(int ox, int oy, int dungeonYStart, int dungeonYEnd, System.Random dungeonRandom)
    {
        foreach (var flower in _flowerTypes)
        {
            for (int y = 1; y < height; y++)
            {
                bool isInDungeonZone = y >= dungeonYStart && y < dungeonYEnd;
                float currentChance = isInDungeonZone ? dungeonFlowerSpawnChance : flower.spawnChance;

                // Use the appropriate random source
                System.Random source = isInDungeonZone ? dungeonRandom : null;

                if (!isInDungeonZone && (height - 1) - y < flower.spawnBelowTop) continue;

                for (int x = 0; x < width; x++)
                {
                    // Check if current cell is already occupied by a tile
                    if (tilemap.GetTile(new Vector3Int(ox + x, oy + y, 0)) != null) continue;

                    TileBase below = tilemap.GetTile(new Vector3Int(ox + x, oy + y - 1, 0));

                    if (below != null && IsStoneOrGroundTile(below))
                    {
                        if (source == null) // Outside dungeon, use Unity's Random
                        {
                            if (Random.value < currentChance)
                            {
                                tilemap.SetTile(new Vector3Int(ox + x, oy + y, 0), flower.flowerTile);
                                caveMap[x, y] = true;
                            }
                        }
                        else // Inside dungeon, use deterministic System.Random
                        {
                            if (source.NextDouble() < currentChance)
                            {
                                tilemap.SetTile(new Vector3Int(ox + x, oy + y, 0), flower.flowerTile);
                                caveMap[x, y] = true;
                            }
                        }
                    }
                }
            }
        }
    }

    // MODIFIED: Now accepts and uses the seeded System.Random for placement inside the dungeon zone.
    void PlaceVanes(int ox, int oy, int dungeonYStart, int dungeonYEnd, System.Random dungeonRandom)
    {
        foreach (var vane in _vaneTypes)
        {
            for (int y = 0; y < height - 1; y++)
            {
                bool isInDungeonZone = y >= dungeonYStart && y < dungeonYEnd;
                float currentChance = isInDungeonZone ? dungeonVaneSpawnChance : vane.spawnChance;

                // Use the appropriate random source
                System.Random source = isInDungeonZone ? dungeonRandom : null;

                if (!isInDungeonZone && (height - 1) - y < vane.spawnBelowTop) continue;

                for (int x = 0; x < width; x++)
                {
                    // Check if current cell is empty AND block above is solid
                    if (tilemap.GetTile(new Vector3Int(ox + x, oy + y, 0)) != null ||
                        tilemap.GetTile(new Vector3Int(ox + x, oy + y + 1, 0)) == null) continue;

                    TileBase aboveTile = tilemap.GetTile(new Vector3Int(ox + x, oy + y + 1, 0));
                    if (IsCrystalite(aboveTile) || IsRoof(aboveTile)) continue;

                    bool shouldSpawn;
                    int columnHeight;

                    if (source == null) // Outside dungeon, use Unity's Random
                    {
                        shouldSpawn = Random.value < currentChance;
                        columnHeight = Random.Range(vane.minColumnHeight, vane.maxColumnHeight + 1);
                    }
                    else // Inside dungeon, use deterministic System.Random
                    {
                        shouldSpawn = source.NextDouble() < currentChance;
                        columnHeight = source.Next(vane.minColumnHeight, vane.maxColumnHeight + 1);
                    }

                    if (shouldSpawn)
                    {
                        for (int i = 0; i < columnHeight; i++)
                        {
                            int targetY = y - i;
                            if (targetY < 0 || tilemap.GetTile(new Vector3Int(ox + x, oy + targetY, 0)) != null) break;

                            Vector3Int vanePos = new Vector3Int(ox + x, oy + targetY, 0);
                            tilemap.SetTile(vanePos, vane.vaneTile);
                        }
                    }
                }
            }
        }
    }


    void PlaceRoofs(int ox, int oy, int dungeonYStart, int dungeonYEnd)
    {
        foreach (var roof in _roofTypes)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (y >= dungeonYStart && y < dungeonYEnd) continue;
                if ((height - 1) - y < roof.spawnBelowTop) continue;

                for (int x = 0; x < width; x++)
                {
                    // Check if current cell is empty
                    if (tilemap.GetTile(new Vector3Int(ox + x, oy + y, 0)) != null) continue;

                    TileBase aboveTile = tilemap.GetTile(new Vector3Int(ox + x, oy + y + 1, 0));

                    if (aboveTile == null || !IsStoneOrGroundTile(aboveTile)) continue;

                    if (Random.value < roof.spawnChance)
                    {
                        Vector3Int cellPos = new Vector3Int(ox + x, oy + y, 0);
                        tilemap.SetTile(cellPos, roof.roofTile);
                    }
                }
            }
        }
    }

    void PlaceCrystalites(int ox, int oy, int dungeonYStart, int dungeonYEnd)
    {
        foreach (var crystal in _crystaliteTypes)
        {
            for (int y = 0; y < height - 1; y++)
            {
                if (y >= dungeonYStart && y < dungeonYEnd) continue;
                if ((height - 1) - y < crystal.spawnBelowTop) continue;

                for (int x = 0; x < width; x++)
                {
                    // Check if current cell is empty AND block above is solid
                    if (tilemap.GetTile(new Vector3Int(ox + x, oy + y, 0)) != null ||
                        tilemap.GetTile(new Vector3Int(ox + x, oy + y + 1, 0)) == null) continue;

                    if (Random.value < crystal.spawnChance)
                    {
                        Vector3Int cellPos = new Vector3Int(ox + x, oy + y, 0);
                        tilemap.SetTile(cellPos, crystal.crystaliteTile);
                        crystalitePositions.Add(cellPos);
                        break;
                    }
                }
            }
        }
    }

    void PlaceEnemies(int ox, int oy, int dungeonYStart, int dungeonYEnd)
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0) return;

        for (int y = 1; y < height - 1; y++)
        {
            if (y >= dungeonYStart && y < dungeonYEnd) continue;
            if ((height - 1) - y < enemySpawnBelowTop) continue;

            for (int x = 1; x < width - 1; x++)
            {
                int worldX = ox + x;
                int worldY = oy + y;

                // Check if the current spot is empty and the block below is solid
                if (tilemap.GetTile(new Vector3Int(worldX, worldY, 0)) == null &&
                    tilemap.GetTile(new Vector3Int(worldX, worldY - 1, 0)) != null)
                {
                    if (Random.value < enemySpawnChance)
                    {
                        Vector3 spawnPos = tilemap.CellToWorld(new Vector3Int(worldX, worldY, 0)) + Vector3.one * 0.5f;
                        Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Count)], spawnPos, Quaternion.identity);
                    }
                }
            }
        }
    }

    void PlaceToppers(int ox, int oy, int dungeonYStart, int dungeonYEnd)
    {
        foreach (var topper in _topperTypes)
        {
            for (int y = 1; y < height; y++)
            {
                if (y >= dungeonYStart && y < dungeonYEnd) continue;
                if ((height - 1) - y < topper.spawnBelowTop) continue;

                for (int x = 0; x < width; x++)
                {
                    // Check if current cell is empty
                    if (tilemap.GetTile(new Vector3Int(ox + x, oy + y, 0)) != null) continue;

                    TileBase belowTile = tilemap.GetTile(new Vector3Int(ox + x, oy + y - 1, 0));

                    if (belowTile == null || !IsStoneOrGroundTile(belowTile)) continue;

                    if (Random.value < topper.spawnChance)
                    {
                        tilemap.SetTile(new Vector3Int(ox + x, oy + y, 0), topper.topperTile);
                    }
                }
            }
        }
    }

    // ---
    // UPDATE METHODS (Crystalite check, UNCHANGED)
    // ---

    void CheckCrystaliteSupport()
    {
        for (int i = crystalitePositions.Count - 1; i >= 0; i--)
        {
            var pos = crystalitePositions[i];
            if (!IsCrystalite(tilemap.GetTile(pos)))
            {
                crystalitePositions.RemoveAt(i);
                continue;
            }

            if (tilemap.GetTile(new Vector3Int(pos.x, pos.y + 1, pos.z)) == null)
            {
                tilemap.SetTile(pos, null);
                crystalitePositions.RemoveAt(i);
                Instantiate(fallingCrystalitePrefab, tilemap.CellToWorld(pos) + Vector3.one * 0.5f, Quaternion.identity);
            }
        }
    }

    void CheckCrystaliteNearPlayer()
    {
        if (player == null) return;

        Vector3 playerPos = player.position;
        for (int i = crystalitePositions.Count - 1; i >= 0; i--)
        {
            var pos = crystalitePositions[i];
            Vector3 worldPos = tilemap.CellToWorld(pos) + Vector3.one * 0.5f;

            if (IsCrystalite(tilemap.GetTile(pos)) &&
                Mathf.Abs(worldPos.x - playerPos.x) <= detectionRangeX &&
                worldPos.y - playerPos.y >= 0 && worldPos.y - playerPos.y <= detectionRangeY &&
                Random.value < fallChanceNearPlayer)
            {
                tilemap.SetTile(pos, null);
                crystalitePositions.RemoveAt(i);
                Instantiate(fallingCrystalitePrefab, worldPos, Quaternion.identity);
            }
        }
    }
}

// ---
// Serializable Classes (Unchanged)
// ---

[System.Serializable]
public class OreTileType
{
    public TileBase oreTile;
    [Range(0f, 1f)] public float spawnChance;
    [Tooltip("Distance in blocks from the top of the tilemap where spawning begins.")]
    public int spawnBelowTop = 100;
}

[System.Serializable]
public class FlowerTileType
{
    public TileBase flowerTile;
    [Range(0f, 1f)] public float spawnChance;
    [Tooltip("Distance in blocks from the top of the tilemap where spawning begins.")]
    public int spawnBelowTop = 100;
}

[System.Serializable]
public class CrystaliteTileType
{
    public TileBase crystaliteTile;
    [Range(0f, 1f)] public float spawnChance;
    [Tooltip("Distance in blocks from the top of the tilemap where spawning begins.")]
    public int spawnBelowTop = 100;
}

[System.Serializable]
public class TopperTileType
{
    public TileBase topperTile;
    [Range(0f, 1f)] public float spawnChance;
    [Tooltip("Distance in blocks from the top of the tilemap where spawning begins.")]
    public int spawnBelowTop = 100;
}

[System.Serializable]
public class RoofTileType
{
    public TileBase roofTile;
    [Range(0f, 1f)] public float spawnChance;
    [Tooltip("Distance in blocks from the top of the tilemap where spawning begins.")]
    public int spawnBelowTop = 100;
}

[System.Serializable]
public class VaneTileType
{
    public TileBase vaneTile;
    [Range(0f, 1f)] public float spawnChance;
    public int minColumnHeight = 1;
    public int maxColumnHeight = 3;
    [Tooltip("Distance in blocks from the top of the tilemap where spawning begins.")]
    public int spawnBelowTop = 100;
}

[System.Serializable]
public class SpecialPrefabType
{
    public GameObject specialPrefab;
    [Range(0f, 1f)] public float spawnChance;
    [Tooltip("Distance in blocks from the top of the tilemap where spawning begins.")]
    public int spawnBelowTop = 100;
}