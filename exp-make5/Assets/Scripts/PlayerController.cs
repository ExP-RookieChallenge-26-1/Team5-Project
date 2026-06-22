using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; // 코루틴 사용을 위해 추가
using UnityEngine.UI; // Image 제어

public class PlayerController : MonoBehaviour
{
    private Animator animator;

    [Header("References")]
    public MapManager mapManager;     // 맵 데이터를 받아올 매니저
    public PlayerStatus playerStatus; // 캐릭터 상태(체력, 부적) 매니저

    // 피격 연출용 변수
    [Header("Effects Settings")]
    public Image damageFlashImage; // 화면을 덮을 빨간색 패널
    public float flashDuration = 0.4f; // 빨간색이 스르륵 사라지는 시간
    public AudioClip oofSound;
    public AudioClip riverWalkSound;

    private Vector2Int mapSize;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    private bool isMoving = false;
    
    private List<Vector2> currentPath = new List<Vector2>(); 
    private Vector2 currentTargetNode; 
    private Vector2Int finalDestination;

    private Vector2Int previousPos;
    
    private bool isKnockbacking = false; // 넉백 연출 중 조작을 막기 위한 상태 플래그

    private bool riverMonologuePlayed = false; // 강 연출을 한 번만 하도록 하기 위한 플래그
    private bool mountainMonologuePlayed = false;

    private class Node
    {
        public Vector2Int Position;
        public int G;   //걸어온 거리 비용
        public int H;   //남은 예상 거리
        public int F { get { return G + H; } } // 총 예상 비용
        public Node Parent; 

        public Node(Vector2Int pos) { Position = pos; }
    }

    void Start()
    {
        animator = GetComponent<Animator>();

        transform.position = new Vector3(0, 0, -1f);
        
        transform.position = new Vector2(0, 0); 
        animator.SetBool("isMoving", false);
        animator.SetFloat("InputX", 0);
        animator.SetFloat("InputY", -1); 
        isMoving = false;
        isKnockbacking = false;
        currentPath.Clear(); 

        if (mapManager != null)
        {
            mapSize = mapManager.GetMapSize();
            playerStatus.InitializeTime(mapManager.currentStageData.maxTime);
        }

        if (damageFlashImage != null)
        {
            Color c = damageFlashImage.color;
            c.a = 0f;
            damageFlashImage.color = c;
        }
    }

    void Update()
    {
        if (StageEventManager.Instance != null && StageEventManager.Instance.isGameOver)
        {
            // 이동 중이었다면 애니메이션도 멈추게 처리
            if (isMoving) 
            {
                animator.SetBool("isMoving", false);
                isMoving = false;
            }
            return; // 아래쪽의 이동이나 마우스 클릭 코드를 실행하지 않고 즉시 종료
        }

        if (isMoving && currentPath.Count > 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(currentTargetNode.x, currentTargetNode.y, transform.position.z), moveSpeed * Time.deltaTime);

            if ((Vector2)transform.position == currentTargetNode)
            {
                // 플레이어가 현재 타일 위에 완전히 발을 디뎠을 때, 향로가 있는 칸이라면 획득 처리
                Vector2Int currentTilePos = new Vector2Int(Mathf.RoundToInt(currentTargetNode.x), Mathf.RoundToInt(currentTargetNode.y));
                if (mapManager != null)
                {
                    mapManager.CollectIncense(currentTilePos);
                }

                currentPath.RemoveAt(0);

                if (currentPath.Count > 0)
                {
                    previousPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
                    SetNextTargetNode(currentPath[0]);
                }
                else
                {
                    isMoving = false;
                    animator.SetBool("isMoving", false);

                    if (mapManager != null && playerStatus != null)
                    {
                        bool canStay = mapManager.OpenTile(finalDestination, playerStatus);
                        if (!canStay)
                        {
                            // transform.position = new Vector3(previousPos.x, previousPos.y, 0);
                            StartCoroutine(MineFailRoutine(finalDestination, previousPos));
                        }
                    }
                }
            }
            return; 
        }

        // 넉백(isKnockbacking) 중일 때는 마우스 클릭 입력을 무시합니다.
        if (!isMoving && !isKnockbacking && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPosition = Mouse.current.position.ReadValue();
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

            CameraController camController = Camera.main.GetComponent<CameraController>();
            if (camController != null && camController.viewMask != null)
            {
                Transform mask = camController.viewMask;
                Vector2 maskPos = mask.position;
                Vector2 maskSize = mask.lossyScale; // CameraController에서 설정한 coreWidth, coreHeight 크기

                // 구멍의 상하좌우 경계선 계산
                float minX = maskPos.x - (maskSize.x / 2f);
                float maxX = maskPos.x + (maskSize.x / 2f);
                float minY = maskPos.y - (maskSize.y / 2f);
                float maxY = maskPos.y + (maskSize.y / 2f);

                // 마우스 클릭 위치가 이 경계선 '바깥'이라면 이동 취소 (무시)
                if (worldPosition.x < minX || worldPosition.x > maxX || 
                    worldPosition.y < minY || worldPosition.y > maxY)
                {
                    return; 
                }
            }

            int targetX = Mathf.RoundToInt(worldPosition.x);
            int targetY = Mathf.RoundToInt(worldPosition.y);
            Vector2Int targetPos = new Vector2Int(targetX, targetY);
            
            if (IsValidTarget(targetPos)) 
            {
                Vector2Int startPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
                FindPath(startPos, targetPos);
            }

        }
    }

    // 지뢰를 밟고 실패했을 때의 연쇄 작용 코루틴
    private IEnumerator MineFailRoutine(Vector2Int minePos, Vector2Int returnPos)
    {
        isKnockbacking = true; // 조작 불가능 상태로 전환

        // 1. 효과음과 함께 화면 번쩍
        if (SoundManager.Instance != null && oofSound != null)
        {
            SoundManager.Instance.PlaySFX(oofSound);
        }

        float timeElapsed = 0;
        while (timeElapsed < flashDuration)
        {
            timeElapsed += Time.deltaTime;
            if (damageFlashImage != null)
            {
                Color flashColor = damageFlashImage.color;
                // 시간에 따라 0.5에서 0으로 서서히 감소
                flashColor.a = Mathf.Lerp(0.5f, 0f, timeElapsed / flashDuration); 
                damageFlashImage.color = flashColor;
            }
            yield return null;
        }

        // 2. 대기 이후에 체력이 깎입니다.
        playerStatus.TakeDamage();

        // 3. 방금 전에 있었던 타일(previousPos)로 돌아갑니다. (스르륵 미끄러지는 넉백 연출)
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(returnPos.x, returnPos.y, -1);
        float slideTimeElapsed = 0;
        float slideDuration = 0.2f; // 뒤로 밀려나는 시간 (짧고 빠르게)

        while (slideTimeElapsed < 1f)
        {
            slideTimeElapsed += Time.deltaTime / slideDuration;
            // 부드러운 곡선(Ease Out) 느낌으로 감속하며 밀려남
            transform.position = Vector3.Lerp(startPos, targetPos, Mathf.Sin(slideTimeElapsed * Mathf.PI * 0.5f));
            yield return null;
        }
        transform.position = targetPos; // 오차 보정

        // 4. 그 뒤 해당 타일이 잠겼다는 것을 표시하도록 업데이트
        mapManager.LockMineTileVisual(minePos);

        isKnockbacking = false; // 연출이 모두 끝나면 다시 조작 가능 상태로 전환
    }

    // 플레이어가 클릭한 곳이 이동할 수 있는 칸인지 확인
    private bool IsValidTarget(Vector2Int target)
    {
        if (mapManager == null) return false;
        if (target.x < 0 || target.x >= mapSize.x || target.y < 0 || target.y >= mapSize.y) return false;
        
        // 1. 이미 열린 타일은 언제든 클릭 가능
        if (mapManager.IsOpened(target.x, target.y)) return true;
        
        // 2. 닫힌 타일이라면, 상하좌우 및 대각선 중 '열린 타일'이 하나라도 붙어있어야 함
        foreach (var n in mapManager.GetNeighbors(target.x, target.y)) 
        {
            if (mapManager.IsOpened(n.x, n.y)) return true;
        }
        
        return false; // 열린 타일과 떨어져 있는 생뚱맞은 닫힌 타일은 클릭 무시
    }

    private void SetNextTargetNode(Vector2 nextNode)
    {
        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(nextNode.x), Mathf.RoundToInt(nextNode.y));

        // 다음으로 가야 할 칸이 수문장이라면, 그 칸으로 올라가지 않고 제자리에서 멈춥니다.
        if (mapManager != null && mapManager.IsGatekeeper(pos.x, pos.y))
        {
            // 1. 수문장 쪽으로 몸 방향만 틀기 (바라보기)
            Vector2 lookDir = nextNode - (Vector2)transform.position;
            animator.SetFloat("InputX", lookDir.x);
            animator.SetFloat("InputY", lookDir.y);

            // 2. 상호작용 이벤트 호출
            mapManager.InteractGatekeeper();

            // 3. 이동 멈춤 (시간 소모도 발생하지 않음)
            currentPath.Clear();
            isMoving = false;
            animator.SetBool("isMoving", false);
            return;
        }
        
        // --- 1. 시간 소모 로직 ---
        int timeCost = 1; // 기본(열린 칸) 1소모
        if (mapManager.IsRiver(pos.x, pos.y) || mapManager.IsMountain(pos.x, pos.y)) timeCost = 10; // 강이면 10소모
        else if (!mapManager.IsOpened(pos.x, pos.y)) timeCost = 3; // 닫힌 칸이면 3소모

        // 시간 소모 시도 (0 이하가 되면 이동을 취소하고 멈춥니다)
        if (!playerStatus.UseTime(timeCost)) 
        {
            currentPath.Clear();
            isMoving = false;
            animator.SetBool("isMoving", false);
            return;
        }

        // --- 2. 향로 시야 감지 로직 ---
        mapManager.UpdateIncenseProximity(pos);

        // --- 3. 강 접근(2칸 이내) 독백 로직 ---
        CheckTerrainProximity(pos);

        // 강 타일 이동 소리
        if (mapManager.IsRiver(pos.x, pos.y) && SoundManager.Instance != null && riverWalkSound != null)
        {   
            SoundManager.Instance.PlaySFX(riverWalkSound);
        }   

        // --- 4. 실제 이동 처리 ---
        currentTargetNode = nextNode;
        Vector2 direction = currentTargetNode - (Vector2)transform.position;
        animator.SetFloat("InputX", direction.x);
        animator.SetFloat("InputY", direction.y);
        animator.SetBool("isMoving", true);
    }

    // 강과 산 타일 근처 2칸 이내인지 확인하고 독백을 호출합니다.
    private void CheckTerrainProximity(Vector2Int playerPos)
    {
        if (mapManager.currentStageData.stageMode != StageMode.Story) return;

        if (!riverMonologuePlayed)
        {
            foreach (Vector2Int rPos in mapManager.currentStageData.riverPositions)
            {
                if (Mathf.Max(Mathf.Abs(rPos.x - playerPos.x), Mathf.Abs(rPos.y - playerPos.y)) <= 2)
                {
                    if (StageEventManager.Instance != null) StageEventManager.Instance.TriggerRiverEvent();
                    riverMonologuePlayed = true; 
                    break;
                }
            }
        }

        if (!mountainMonologuePlayed)
        {
            foreach (Vector2Int mPos in mapManager.currentStageData.mountainPositions)
            {
                if (Mathf.Max(Mathf.Abs(mPos.x - playerPos.x), Mathf.Abs(mPos.y - playerPos.y)) <= 2)
                {
                    if (StageEventManager.Instance != null) StageEventManager.Instance.TriggerMountainEvent();
                    mountainMonologuePlayed = true; 
                    break;
                }
            }
        }
    }

    private void FindPath(Vector2Int startPos, Vector2Int targetPos)
    {
        if (startPos == targetPos) return;

        previousPos = startPos;

        List<Node> openList = new List<Node>();
        HashSet<Vector2Int> closedList = new HashSet<Vector2Int>();

        Node startNode = new Node(startPos);
        openList.Add(startNode);

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (openList.Count > 0)
        {
            Node currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].F < currentNode.F || (openList[i].F == currentNode.F && openList[i].H < currentNode.H))
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode.Position);

            if (currentNode.Position == targetPos)
            {
                finalDestination = targetPos; // 도착 시 맵과 상호작용하기 위해 위치 기억
                RetracePath(startNode, currentNode);
                return;
            }

            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighborPos = currentNode.Position + dir;

                if (neighborPos.x < 0 || neighborPos.x >= mapSize.x || neighborPos.y < 0 || neighborPos.y >= mapSize.y) continue;
                if (closedList.Contains(neighborPos)) continue;

                if (mapManager != null)
                {
                    // 수문장 제외
                    if (mapManager.IsGatekeeper(neighborPos.x, neighborPos.y) && neighborPos != targetPos) continue;
                    // 지뢰 해제 실패로 생성된 '영구 장애물' 칸은 탐색에서 무조건 제외
                    if (!mapManager.IsWalkable(neighborPos.x, neighborPos.y)) continue;

                    // 아직 까보지 않은 닫힌 타일인 경우
                    if (!mapManager.IsOpened(neighborPos.x, neighborPos.y))
                    {
                        // 내가 클릭한 최종 목적지가 아니라면, 가는 길 중간에 닫힌 타일을 밟을 순 없음
                        if (neighborPos != targetPos) continue; 
                    }
                }
                // A* 알고리즘의 이동 비용(Cost)에도 시간 패널티를 그대로 적용하여 가장 시간이 적게 드는 길을 찾게 만듭니다.
                int stepCost = 1; 
                if (mapManager.IsRiver(neighborPos.x, neighborPos.y) || mapManager.IsMountain(neighborPos.x, neighborPos.y)) stepCost = 10;
                else if (!mapManager.IsOpened(neighborPos.x, neighborPos.y)) stepCost = 3;

                int newMovementCostToNeighbor = currentNode.G + stepCost; 
                Node neighborNode = openList.Find(n => n.Position == neighborPos);

                if (neighborNode == null || newMovementCostToNeighbor < neighborNode.G)
                {
                    if (neighborNode == null)
                    {
                        neighborNode = new Node(neighborPos);
                        openList.Add(neighborNode);
                    }

                    neighborNode.G = newMovementCostToNeighbor;
                    neighborNode.H = Mathf.Abs(neighborPos.x - targetPos.x) + Mathf.Abs(neighborPos.y - targetPos.y); 
                    neighborNode.Parent = currentNode;
                }
            }
        }
    }

    private void RetracePath(Node startNode, Node endNode)
    {
        List<Vector2> path = new List<Vector2>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(new Vector2(currentNode.Position.x, currentNode.Position.y));
            currentNode = currentNode.Parent;
        }

        path.Reverse(); 
        currentPath = path;

        if (currentPath.Count > 0)
        {
            isMoving = true;
            SetNextTargetNode(currentPath[0]);
        }
    }
}