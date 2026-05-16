using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MapManager : MonoBehaviour
{
    // 맵 생성에 필요한 데이터를 담은 스크립터블 오브젝트
    [Header("Stage Data")]
    public StageData currentStageData;

    [Header("Prefabs")]
    public GameObject closedTilePrefab;
    public GameObject openTilePrefab;
    public GameObject redMinePrefab;
    public GameObject blueMinePrefab;
    public GameObject hintTextPrefab;
    public GameObject lockedRedMinePrefab;
    public GameObject lockedBlueMinePrefab;

    // 스토리 모드용 프리팹
    [Header("Story Prefabs")]
    public GameObject riverPrefab;
    public GameObject gatekeeperPrefab;

    public GameObject faintIncensePrefab; // 미확인 타일 위에 띄울 연한 향로
    public GameObject realIncensePrefab;  // 획득 가능한 완전한 모습의 향로

    private TileData[,] grid;

    private int foundIncenseCount = 0;

    public struct TileData {
        public bool isRedMine;
        public bool isBlueMine;
        public bool isOpened;
        public bool isObstacle; // 해제 실패 시
        public int blueNeighbors;
        public int redNeighbors;
        public GameObject visualObj;

        // 스토리 모드용 속성
        public bool isRiver;
        public bool isIncense;
        public bool isGatekeeper;

        public GameObject faintIncenseVisual;
        public GameObject realIncenseVisual;
    }

    // 데이터를 먼저 확인하고 맵 생성
    void Awake() 
    { 
        if (currentStageData == null)
        {
            Debug.LogError("StageData가 연결되지 않았습니다!");
            return;
        }
        GenerateMap(); 
    }

    private void GenerateMap()
    {
        // StageData의 크기를 바탕으로 그리드 생성
        grid = new TileData[currentStageData.mapWidth, currentStageData.mapHeight];
        foundIncenseCount = 0;
        
        // 스토리 모드라면 강, 향로, NPC 위치를 먼저 세팅 (지뢰가 이 위를 덮어쓰지 않도록)
        if (currentStageData.stageMode == StageMode.Story)
        {
            PlaceStoryElements();
        }

        PlaceMines();
        CalculateHints();
        SpawnInitialTiles();
        
        // 시작점(플레이어 시작 위치)은 미리 열어둡니다.
        OpenTile(currentStageData.playerStartPosition, null, true);
    }

    // 스토리 요소 배치
    private void PlaceStoryElements()
    {
        foreach (Vector2Int pos in currentStageData.riverPositions) {
            if (IsValidPos(pos.x, pos.y)) grid[pos.x, pos.y].isRiver = true;
        }
        foreach (Vector2Int pos in currentStageData.incensePositions) {
            if (IsValidPos(pos.x, pos.y)) grid[pos.x, pos.y].isIncense = true;
        }
        if (IsValidPos(currentStageData.gatekeeperPosition.x, currentStageData.gatekeeperPosition.y)) {
            grid[currentStageData.gatekeeperPosition.x, currentStageData.gatekeeperPosition.y].isGatekeeper = true;
        }
    }

    // 구역을 나누어 지뢰 배치
    private void PlaceMines()
    {
        // 1. 왼쪽 구역 배치 (0 ~ divideX - 1)
        PlaceMinesInArea(0, currentStageData.divideX - 1, currentStageData.leftRedMineCount, currentStageData.leftBlueMineCount);
        
        // 2. 오른쪽 구역 배치 (divideX ~ mapWidth - 1)
        PlaceMinesInArea(currentStageData.divideX, currentStageData.mapWidth - 1, currentStageData.rightRedMineCount, currentStageData.rightBlueMineCount);
    }

    // 특정 X좌표 범위 내에 지뢰를 랜덤 배치하는 함수
    private void PlaceMinesInArea(int minX, int maxX, int redCount, int blueCount)
    {
        int placedRed = 0;
        int placedBlue = 0;

        // 무한 루프 방지용 (안전장치)
        int maxAttempts = 1000; 
        int attempts = 0;

        while ((placedRed < redCount || placedBlue < blueCount) && attempts < maxAttempts)
        {
            attempts++;
            int rx = Random.Range(minX, maxX + 1);
            int ry = Random.Range(0, currentStageData.mapHeight);

            // 시작점 및 주변(보호 구역), 이미 지뢰가 있거나, 스토리 요소(강, 향로, NPC)가 있는 곳은 건너뜀
            if (Mathf.Abs(rx - currentStageData.playerStartPosition.x) <= 1 && Mathf.Abs(ry - currentStageData.playerStartPosition.y) <= 1) continue;
            if (grid[rx, ry].isRedMine || grid[rx, ry].isBlueMine) continue;
            if (grid[rx, ry].isRiver || grid[rx, ry].isIncense || grid[rx, ry].isGatekeeper) continue;

            if (placedRed < redCount) { grid[rx, ry].isRedMine = true; placedRed++; }
            else if (placedBlue < blueCount) { grid[rx, ry].isBlueMine = true; placedBlue++; }
        }
    }

    // 힌트 계산 시 mapWidth, mapHeight 사용
    private void CalculateHints()
    {
        for (int x = 0; x < currentStageData.mapWidth; x++) {
            for (int y = 0; y < currentStageData.mapHeight; y++) {
                if (grid[x, y].isRedMine || grid[x, y].isBlueMine) continue;
                
                foreach (var n in GetNeighbors(x, y)) {
                    if (grid[n.x, n.y].isRedMine) grid[x, y].redNeighbors++;
                    if (grid[n.x, n.y].isBlueMine) grid[x, y].blueNeighbors++;
                }
            }
        }
    }

    // 타일 생성 시 mapWidth, mapHeight 사용 및 스토리 타일 시각화
    private void SpawnInitialTiles()
    {
        for (int x = 0; x < currentStageData.mapWidth; x++) {
            for (int y = 0; y < currentStageData.mapHeight; y++) {
                grid[x, y].visualObj = Instantiate(closedTilePrefab, new Vector3(x, y, 0), Quaternion.identity, transform);
                
                // 임시로 강 타일임을 표시 (나중에 프리팹으로 교체 가능)
                if (grid[x, y].isRiver && riverPrefab != null) {
                    Instantiate(riverPrefab, new Vector3(x, y, -0.1f), Quaternion.identity, transform);
                }
                // 수문장 시각화
                if (grid[x, y].isGatekeeper && gatekeeperPrefab != null) {
                    Instantiate(gatekeeperPrefab, new Vector3(x, y, -0.1f), Quaternion.identity, transform);
                }
            }
        }
    }

    // 플레이어 위치를 기반으로 3칸 이내의 향로를 감지해 반투명하게 띄웁니다.
    public void UpdateIncenseProximity(Vector2Int playerPos)
    {
        if (currentStageData.stageMode != StageMode.Story) return;

        foreach (Vector2Int pos in currentStageData.incensePositions)
        {
            if (grid[pos.x, pos.y].isOpened) continue; // 이미 열린 곳은 무시

            // 가로, 세로 중 더 먼 거리를 기준으로 3칸 이내인지 확인 (Chebyshev distance)
            int dist = Mathf.Max(Mathf.Abs(pos.x - playerPos.x), Mathf.Abs(pos.y - playerPos.y));
            
            // 3칸 이내이고, 아직 반투명 향로를 안 띄웠다면 생성
            if (dist <= 3 && grid[pos.x, pos.y].faintIncenseVisual == null && faintIncensePrefab != null)
            {
                grid[pos.x, pos.y].faintIncenseVisual = Instantiate(faintIncensePrefab, new Vector3(pos.x, pos.y, -0.2f), Quaternion.identity, transform);
            }
        }
    }

    public void InteractGatekeeper()
    {
        if (foundIncenseCount >= currentStageData.incensePositions.Count)
        {
            if (StageEventManager.Instance != null) StageEventManager.Instance.TriggerStageClear();
        }
        else
        {
            Debug.Log($"향로가 부족합니다! (현재 {foundIncenseCount} / 필요 {currentStageData.incensePositions.Count})");
        }
    }

    // 💡 [추가됨] 플레이어가 진짜 향로 타일 위에 올라왔을 때 호출되어 향로를 획득하고 없애는 함수
    public void CollectIncense(Vector2Int pos)
    {
        if (grid[pos.x, pos.y].isIncense)
        {
            grid[pos.x, pos.y].isIncense = false; // 더 이상 향로 타일이 아님 처리
            
            if (grid[pos.x, pos.y].realIncenseVisual != null)
            {
                Destroy(grid[pos.x, pos.y].realIncenseVisual); // 향로 이미지 파괴
            }

            foundIncenseCount++;
            if (StageEventManager.Instance != null)
            {
                StageEventManager.Instance.TriggerIncenseFound(foundIncenseCount);
            }
            Debug.Log($"향로 획득! 현재 개수: {foundIncenseCount}");
        }
    }

    public bool OpenTile(Vector2Int pos, PlayerStatus player, bool isSilent = false)
    {

        if (grid[pos.x, pos.y].isOpened || grid[pos.x, pos.y].isObstacle) return true;

        if (!isSilent && (grid[pos.x, pos.y].isRedMine || grid[pos.x, pos.y].isBlueMine))
        {
            bool success = player.HandleMineEncounter(grid[pos.x, pos.y].isRedMine);
            if (!success) {
                grid[pos.x, pos.y].isObstacle = true;
                return false;
            }
        }

        grid[pos.x, pos.y].isOpened = true;
        UpdateTileVisual(pos, "Open");

        if (grid[pos.x, pos.y].redNeighbors == 0 && grid[pos.x, pos.y].blueNeighbors == 0) {
            foreach (var n in GetNeighbors(pos.x, pos.y)) {
                // 강 타일은 연쇄 반응으로 자동으로 열리지 않도록 막음 (직접 밟아야 함)
                if (!grid[n.x, n.y].isRedMine && !grid[n.x, n.y].isBlueMine && !grid[n.x, n.y].isRiver) OpenTile(n, player, true);
            }
        }
        return true;
    }

    public void LockMineTileVisual(Vector2Int pos)
    {
        string lockedType = grid[pos.x, pos.y].isRedMine ? "LockedRed" : "LockedBlue";
        UpdateTileVisual(pos, lockedType); 
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

            // 타일이 열렸는데 향로 자리라면 완전한 향로를 띄움
            if (grid[pos.x, pos.y].isIncense && realIncensePrefab != null)
            {
                // 반투명 향로가 있었다면 파괴
                if (grid[pos.x, pos.y].faintIncenseVisual != null) Destroy(grid[pos.x, pos.y].faintIncenseVisual);
                
                // 생성된 오브젝트를 변수에 저장해 둡니다 (나중에 캐릭터가 밟으면 지우기 위해)
                grid[pos.x, pos.y].realIncenseVisual = Instantiate(realIncensePrefab, new Vector3(pos.x, pos.y, -0.2f), Quaternion.identity, grid[pos.x, pos.y].visualObj.transform);
            }
        }
        else if (type == "LockedRed") 
        {
            Instantiate(lockedRedMinePrefab, new Vector3(pos.x, pos.y, -0.1f), Quaternion.identity, grid[pos.x, pos.y].visualObj.transform);
        }
        else if (type == "LockedBlue") 
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
                // 💡 [수정됨] mapWidth, mapHeight 사용
                if (nx >= 0 && nx < currentStageData.mapWidth && ny >= 0 && ny < currentStageData.mapHeight) n.Add(new Vector2Int(nx, ny));
            }
        }
        return n;
    }

    public Vector2Int GetMapSize() => new Vector2Int(currentStageData.mapWidth, currentStageData.mapHeight);
    public bool IsWalkable(int x, int y) => !grid[x, y].isObstacle;
    public bool IsOpened(int x, int y) => grid[x, y].isOpened;
    public bool IsRiver(int x, int y) => grid[x, y].isRiver;
    public bool IsGatekeeper(int x, int y) => grid[x, y].isGatekeeper;
    private bool IsValidPos(int x, int y) => x >= 0 && x < currentStageData.mapWidth && y >= 0 && y < currentStageData.mapHeight;
}