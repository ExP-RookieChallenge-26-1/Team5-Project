using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Animator animator;

    [Header("References")]
    public MapManager mapManager;     // 맵 데이터를 받아올 매니저
    public PlayerStatus playerStatus; // 캐릭터 상태(체력, 부적) 매니저

    private Vector2Int mapSize;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    private bool isMoving = false;
    
    private List<Vector2> currentPath = new List<Vector2>(); 
    private Vector2 currentTargetNode; 
    private Vector2Int finalDestination;

    private Vector2Int previousPos;

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
        
        transform.position = new Vector2(0, 0); 
        animator.SetBool("isMoving", false);
        animator.SetFloat("InputX", 0);
        animator.SetFloat("InputY", -1); 
        isMoving = false;
        currentPath.Clear(); 

        if (mapManager != null)
        {
            mapSize = mapManager.GetMapSize();
        }
    }

    void Update()
    {
        if (isMoving && currentPath.Count > 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(currentTargetNode.x, currentTargetNode.y, transform.position.z), moveSpeed * Time.deltaTime);

            if ((Vector2)transform.position == currentTargetNode)
            {
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
                            transform.position = new Vector3(previousPos.x, previousPos.y, 0);
                        }
                    }
                }
            }
            return; 
        }

        if (!isMoving && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPosition = Mouse.current.position.ReadValue();
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

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
        currentTargetNode = nextNode;
        Vector2 direction = currentTargetNode - (Vector2)transform.position;

        animator.SetFloat("InputX", direction.x);
        animator.SetFloat("InputY", direction.y);
        animator.SetBool("isMoving", true);
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
                    // 지뢰 해제 실패로 생성된 '영구 장애물' 칸은 탐색에서 무조건 제외
                    if (!mapManager.IsWalkable(neighborPos.x, neighborPos.y)) continue;

                    // 아직 까보지 않은 닫힌 타일인 경우
                    if (!mapManager.IsOpened(neighborPos.x, neighborPos.y))
                    {
                        // 내가 클릭한 최종 목적지가 아니라면, 가는 길 중간에 닫힌 타일을 밟을 순 없음
                        if (neighborPos != targetPos) continue; 
                    }
                }

                int newMovementCostToNeighbor = currentNode.G + 1; 
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