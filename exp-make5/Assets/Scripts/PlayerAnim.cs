using System.Collections.Generic;
using UnityEngine;

// 플레이어가 이동하는 방향에 따라 스프라이트를 교체해 애니메이션을 재생합니다.
// 각 방향(앞/뒤/좌/우)마다 스프라이트 리스트를 넣으면, 일정 주기(frameInterval)마다
// 리스트 안의 스프라이트가 순서대로 바뀌며 걷는 듯한 연출이 됩니다.
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerAnim : MonoBehaviour
{
    [Header("References")]
    [Tooltip("스프라이트가 바뀌는 주체 (비우면 자동으로 같은 오브젝트의 SpriteRenderer 사용)")]
    public SpriteRenderer spriteRenderer;

    [Header("Direction Sprites")]
    [Tooltip("앞(아래)으로 갈 때 순환할 스프라이트들")]
    public List<Sprite> frontSprites = new List<Sprite>();
    [Tooltip("뒤(위)로 갈 때 순환할 스프라이트들")]
    public List<Sprite> backSprites = new List<Sprite>();
    [Tooltip("왼쪽으로 갈 때 순환할 스프라이트들")]
    public List<Sprite> leftSprites = new List<Sprite>();
    [Tooltip("오른쪽으로 갈 때 순환할 스프라이트들")]
    public List<Sprite> rightSprites = new List<Sprite>();

    [Header("Animation Settings")]
    [Tooltip("스프라이트가 다음 장면으로 바뀌는 주기(초). 작을수록 빠르게 바뀝니다.")]
    public float frameInterval = 0.2f;

    [Tooltip("멈춰 있을 때도 애니메이션을 계속 재생할지 여부 (꺼두면 멈추면 첫 프레임으로 고정)")]
    public bool animateWhenIdle = false;

    private enum Dir { Front, Back, Left, Right }
    private Dir currentDir = Dir.Front;
    private bool isMoving = false;

    private int frameIndex = 0;
    private float timer = 0f;
    private Vector3 lastPosition;

    void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        lastPosition = transform.position;
    }

    void Update()
    {
        // 1. 이전 프레임과의 위치 차이로 이동 여부 / 방향을 감지합니다.
        Vector3 delta = transform.position - lastPosition;
        lastPosition = transform.position;

        isMoving = delta.sqrMagnitude > 0.000001f;
        if (isMoving) UpdateDirection(delta.x, delta.y);

        // 2. 현재 방향에 맞는 스프라이트로 프레임을 진행합니다.
        Animate();
    }

    // 가로/세로 이동량 중 더 큰 축을 기준으로 바라보는 방향을 정합니다.
    private void UpdateDirection(float dx, float dy)
    {
        if (Mathf.Abs(dx) >= Mathf.Abs(dy))
            currentDir = dx >= 0f ? Dir.Right : Dir.Left;
        else
            currentDir = dy >= 0f ? Dir.Back : Dir.Front;
    }

    private void Animate()
    {
        List<Sprite> sprites = GetSprites(currentDir);
        if (sprites == null || sprites.Count == 0) return;

        // 멈춰 있고, 정지 시 애니메이션을 끄기로 했다면 첫 프레임(정지 자세)으로 고정합니다.
        if (!isMoving && !animateWhenIdle)
        {
            timer = 0f;
            frameIndex = 0;
            spriteRenderer.sprite = sprites[0];
            return;
        }

        // 주기가 지날 때마다 다음 스프라이트로 순환합니다.
        timer += Time.deltaTime;
        if (timer >= frameInterval && frameInterval > 0f)
        {
            timer -= frameInterval;
            frameIndex = (frameIndex + 1) % sprites.Count;
        }

        // 방향이 바뀌어 리스트 길이가 달라졌을 때를 대비한 안전 처리
        if (frameIndex >= sprites.Count) frameIndex = 0;

        spriteRenderer.sprite = sprites[frameIndex];
    }

    private List<Sprite> GetSprites(Dir dir)
    {
        switch (dir)
        {
            case Dir.Back: return backSprites;
            case Dir.Left: return leftSprites;
            case Dir.Right: return rightSprites;
            default: return frontSprites; // Dir.Front
        }
    }

    // PlayerController 등 외부에서 방향을 직접 지정하고 싶을 때 호출할 수 있는 공개 메서드입니다.
    // (위치 변화 자동 감지 대신, 바라보는 방향을 강제로 정하고 싶을 때 사용)
    public void SetDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.000001f) return;
        UpdateDirection(dir.x, dir.y);
    }
}
