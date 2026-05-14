using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MapManager : MonoBehaviour
{
    [Header("Map Settings")]
    public int width = 18;
    public int height = 14;
    public int redMineCount = 5;
    public int blueMineCount = 5;

    [Header("Prefabs")]
    public GameObject closedTilePrefab;
    public GameObject openTilePrefab;
    public GameObject redMinePrefab;
    public GameObject blueMinePrefab;
    public GameObject hintTextPrefab;
    public GameObject lockedRedMinePrefab;
    public GameObject lockedBlueMinePrefab;

    private TileData[,] grid;

    public struct TileData {
        public bool isRedMine;
        public bool isBlueMine;
        public bool isOpened;
        public bool isObstacle; // 해제 실패 시
        public int blueNeighbors;
        public int redNeighbors;
        public GameObject visualObj;
    }

    void Awake() { GenerateMap(); }

    private void GenerateMap()
    {
        grid = new TileData[width, height];
        PlaceMines();
        CalculateHints();
        SpawnInitialTiles();
        
        // 시작점(0,0)은 미리 열어둡니다.
        OpenTile(Vector2Int.zero, null, true);
    }

    private void PlaceMines()
    {
        int placedRed = 0;
        int placedBlue = 0;

        while (placedRed < redMineCount || placedBlue < blueMineCount)
        {
            int rx = Random.Range(0, width);
            int ry = Random.Range(0, height);

            // 시작 2x2 구역 및 중복 방지
            if ((rx <= 1 && ry <= 1) || grid[rx, ry].isRedMine || grid[rx, ry].isBlueMine) continue;

            if (placedRed < redMineCount) { grid[rx, ry].isRedMine = true; placedRed++; }
            else if (placedBlue < blueMineCount) { grid[rx, ry].isBlueMine = true; placedBlue++; }
        }
    }

    private void CalculateHints()
    {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (grid[x, y].isRedMine || grid[x, y].isBlueMine) continue;
                
                foreach (var n in GetNeighbors(x, y)) {
                    if (grid[n.x, n.y].isRedMine) grid[x, y].redNeighbors++;
                    if (grid[n.x, n.y].isBlueMine) grid[x, y].blueNeighbors++;
                }
            }
        }
    }

    private void SpawnInitialTiles()
    {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                grid[x, y].visualObj = Instantiate(closedTilePrefab, new Vector3(x, y, 0), Quaternion.identity, transform);
            }
        }
    }

    // 캐릭터가 타일을 밟았을 때 호출 (넉백 여부 반환)
    public bool OpenTile(Vector2Int pos, PlayerStatus player, bool isSilent = false)
    {
        if (grid[pos.x, pos.y].isOpened || grid[pos.x, pos.y].isObstacle) return true;

        // 지뢰 판별
        if (!isSilent && (grid[pos.x, pos.y].isRedMine || grid[pos.x, pos.y].isBlueMine))
        {
            bool success = player.HandleMineEncounter(grid[pos.x, pos.y].isRedMine);
            if (!success) {
                grid[pos.x, pos.y].isObstacle = true;

                string lockedType = grid[pos.x, pos.y].isRedMine ? "LockedRed" : "LockedBlue";
                UpdateTileVisual(pos, lockedType); 
                
                return false;
            }
        }

        // 일반 타일 오픈 및 연쇄 반응
        grid[pos.x, pos.y].isOpened = true;
        UpdateTileVisual(pos, "Open");

        if (grid[pos.x, pos.y].redNeighbors == 0 && grid[pos.x, pos.y].blueNeighbors == 0) {
            foreach (var n in GetNeighbors(pos.x, pos.y)) {
                if (!grid[n.x, n.y].isRedMine && !grid[n.x, n.y].isBlueMine) OpenTile(n, player, true);
            }
        }
        return true;
    }

    private void UpdateTileVisual(Vector2Int pos, string type)
    {
        Destroy(grid[pos.x, pos.y].visualObj);
        GameObject prefab = (type == "closed") ? closedTilePrefab : openTilePrefab; 
        
        grid[pos.x, pos.y].visualObj = Instantiate(prefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, transform);
        
        if (type == "Open") 
        {
            if (grid[pos.x, pos.y].isRedMine) Instantiate(redMinePrefab, new Vector3(pos.x, pos.y, -0.1f), Quaternion.identity, grid[pos.x, pos.y].visualObj.transform);
            else if (grid[pos.x, pos.y].isBlueMine) Instantiate(blueMinePrefab, new Vector3(pos.x, pos.y, -0.1f), Quaternion.identity, grid[pos.x, pos.y].visualObj.transform);
            else ShowHint(pos);
        }
        else if (type == "LockedRed") // 💡 부적 없이 빨간 지뢰 밟음
        {
            Instantiate(lockedRedMinePrefab, new Vector3(pos.x, pos.y, -0.1f), Quaternion.identity, grid[pos.x, pos.y].visualObj.transform);
        }
        else if (type == "LockedBlue") // 💡 부적 없이 파란 지뢰 밟음
        {
            Instantiate(lockedBlueMinePrefab, new Vector3(pos.x, pos.y, -0.1f), Quaternion.identity, grid[pos.x, pos.y].visualObj.transform);
        }
    }

    private void ShowHint(Vector2Int pos) {
        if (grid[pos.x, pos.y].redNeighbors == 0 && grid[pos.x, pos.y].blueNeighbors == 0) return;

        GameObject hint = Instantiate(hintTextPrefab, new Vector3(pos.x, pos.y, -0.2f), Quaternion.identity, grid[pos.x, pos.y].visualObj.transform);
        hint.GetComponentInChildren<TextMeshPro>().text = $"<color=#55AAFF>{grid[pos.x, pos.y].blueNeighbors}</color>/<color=#FF5555>{grid[pos.x, pos.y].redNeighbors}</color>";
    }

    public List<Vector2Int> GetNeighbors(int x, int y) {
        List<Vector2Int> n = new List<Vector2Int>();
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                if (i == 0 && j == 0) continue;
                int nx = x + i, ny = y + j;
                if (nx >= 0 && nx < width && ny >= 0 && ny < height) n.Add(new Vector2Int(nx, ny));
            }
        }
        return n;
    }

    public Vector2Int GetMapSize() => new Vector2Int(width, height);
    public bool IsWalkable(int x, int y) => !grid[x, y].isObstacle;
    public bool IsOpened(int x, int y) => grid[x, y].isOpened;
}