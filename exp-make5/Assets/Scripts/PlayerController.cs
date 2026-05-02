using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Animator animator;

    [Header("Map Settings")]
    public int mapSize = 10; 
    
    // 타일들을 저장해둘 2차원 배열 추가
    private GameObject[,] mapTiles; 

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    private bool isMoving = false;
    
    private List<Vector2> currentPath = new List<Vector2>(); 
    private Vector2 currentTargetNode; 

    // 타일 하이라이트 상태 관리를 위한 변수 추가
    private Vector2Int currentYellowTilePos;
    private bool isTileHighlighted = false;

    private class Node
    {
        public Vector2Int Position;
        public int G; 
        public int H; 
        public int F { get { return G + H; } } 
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

        GenerateTemporaryMap(mapSize);
    }

    public void GenerateTemporaryMap(int n)
    {
        mapSize = n;
        mapTiles = new GameObject[n, n]; 

        Shader spriteShader = Shader.Find("Sprites/Default");
        Material unlitMaterial = new Material(spriteShader);

        for (int x = 0; x < n; x++)
        {
            for (int y = 0; y < n; y++)
            {
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tile.transform.position = new Vector3(x, y, 1f);
                tile.name = $"TempTile_{x}_{y}";
                
                MeshRenderer renderer = tile.GetComponent<MeshRenderer>();
                renderer.material = unlitMaterial; 
                
                Color tileColor = ((x + y) % 2 == 0) ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.6f, 0.6f, 0.6f);
                renderer.material.color = tileColor;
                
                Destroy(tile.GetComponent<MeshCollider>());

                mapTiles[x, y] = tile; 
            }
        }
    }

    void Update()
    {
        if (isMoving && currentPath.Count > 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, currentTargetNode, moveSpeed * Time.deltaTime);

            if ((Vector2)transform.position == currentTargetNode)
            {
                currentPath.RemoveAt(0); 

                if (currentPath.Count > 0)
                {
                    SetNextTargetNode(currentPath[0]);
                }
                else
                {
                    isMoving = false;
                    animator.SetBool("isMoving", false);
                    
                    // 목적지 도착 시 타일 색상 원상 복구
                    if (isTileHighlighted) RevertTileColor(currentYellowTilePos);
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

            if (targetX >= 0 && targetX < mapSize && targetY >= 0 && targetY < mapSize)
            {
                Vector2Int startPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
                Vector2Int targetPos = new Vector2Int(targetX, targetY);

                FindPath(startPos, targetPos);
            }
        }
    }

    private void SetNextTargetNode(Vector2 nextNode)
    {
        currentTargetNode = nextNode;
        Vector2 direction = currentTargetNode - (Vector2)transform.position;

        animator.SetFloat("InputX", direction.x);
        animator.SetFloat("InputY", direction.y);
        animator.SetBool("isMoving", true);
    }

    // 도착점 타일을 노란색으로 변경하는 함수
    private void HighlightTile(Vector2Int pos)
    {
        currentYellowTilePos = pos;
        isTileHighlighted = true;
        mapTiles[pos.x, pos.y].GetComponent<MeshRenderer>().material.color = Color.yellow;
    }

    // 타일을 원래 체스판 색상으로 되돌리는 함수
    private void RevertTileColor(Vector2Int pos)
    {
        Color originalColor = ((pos.x + pos.y) % 2 == 0) ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.6f, 0.6f, 0.6f);
        mapTiles[pos.x, pos.y].GetComponent<MeshRenderer>().material.color = originalColor;
        isTileHighlighted = false;
    }

    private void FindPath(Vector2Int startPos, Vector2Int targetPos)
    {
        if (startPos == targetPos) return;

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
                RetracePath(startNode, currentNode);
                return;
            }

            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighborPos = currentNode.Position + dir;

                if (neighborPos.x < 0 || neighborPos.x >= mapSize || neighborPos.y < 0 || neighborPos.y >= mapSize) continue;
                if (closedList.Contains(neighborPos)) continue;

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
            // 길찾기 성공 시 가장 마지막 노드(도착점)를 노란색으로 칠함
            HighlightTile(endNode.Position);
            
            isMoving = true;
            SetNextTargetNode(currentPath[0]);
        }
    }
}