using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public Transform target;       // 카메라가 따라다닐 타겟 (플레이어)
    public MapManager mapManager;  // 맵 크기 정보를 가져올 매니저

    [Header("Movement Settings")]
    public float smoothTime = 0.3f; // 카메라 이동 부드러움
    private Vector3 currentVelocity = Vector3.zero;

    [Header("View Settings")]
    // 💡 화면의 기준이 될 칸 수 
    public float targetViewWidth = 14f;  // 종스크롤 시 꽉 채울 가로 기준
    public float targetViewHeight = 8f;  // 횡스크롤 시 꽉 채울 세로 기준

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.backgroundColor = Color.black; 
    }

    void LateUpdate()
    {
        if (target == null || mapManager == null) return;

        Vector2Int mapSize = mapManager.GetMapSize();
        float mapWidth = mapSize.x;  
        float mapHeight = mapSize.y; 

        // 1. 💡 [핵심 수정] 맵의 실제 크기가 우리의 '기준 크기(14, 8)' 대비 어느 쪽이 더 긴지(비율) 계산합니다.
        float ratioX = mapWidth / targetViewWidth;
        float ratioY = mapHeight / targetViewHeight;

        // 가로가 기준치보다 상대적으로 더 길다 (또는 같다) -> 횡스크롤 베이스
        if (ratioX >= ratioY)
        {
            // 세로를 무조건 목표치(8)에 맞춰 화면 위아래를 꽉 채웁니다.
            cam.orthographicSize = targetViewHeight / 2f;
        }
        // 세로가 기준치보다 상대적으로 더 길다 -> 종스크롤 베이스
        else
        {
            // 가로를 무조건 목표치(14)에 맞춰 화면 양옆을 꽉 채웁니다.
            cam.orthographicSize = (targetViewWidth / cam.aspect) / 2f;
        }

        // 2. 카메라가 비추는 실제 영역 크기(절반) 계산
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        // 3. 목표 위치 계산
        Vector3 targetPos = new Vector3(target.position.x, target.position.y, -10f);

        // 4. 맵 경계 제한 (Clamping)
        float clampedX = CalculateAxisPosition(targetPos.x, mapWidth, halfWidth);
        float clampedY = CalculateAxisPosition(targetPos.y, mapHeight, halfHeight);

        Vector3 desiredPosition = new Vector3(clampedX, clampedY, -10f);

        // 5. 부드러운 이동
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
    }

    private float CalculateAxisPosition(float targetCoord, float mapSpan, float viewHalfSpan)
    {
        float mapMin = -0.5f; 
        float mapMax = mapSpan - 0.5f;

        // 맵 크기가 카메라 시야보다 작거나 같으면 정중앙에 고정 (오차 보정 0.01f 포함)
        if (mapSpan <= (viewHalfSpan * 2f) + 0.01f) 
        {
            return (mapMin + mapMax) / 2f;
        }

        // 맵이 시야보다 크면 캐릭터 추적 및 맵 바깥 여백 노출 방지
        return Mathf.Clamp(targetCoord, mapMin + viewHalfSpan, mapMax - viewHalfSpan);
    }
}