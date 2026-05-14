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
    public float coreWidth = 12f;  // 화면에 절대적으로 보장할 맵의 가로 칸 수
    public float coreHeight = 8f;  // 화면에 절대적으로 보장할 맵의 세로 칸 수

    [Header("UI Padding Settings")]
    [Range(0f, 0.5f)]
    public float paddingRatio = 0.1f;

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

        // 1. 💡 카메라 줌(Zoom) 설정: '코어 영역(12x8) + 상하좌우 여백(10%)'을 포함한 최종 시야 계산
        float paddedHeight = coreHeight * (1f + paddingRatio * 2f);
        float paddedWidth = coreWidth * (1f + paddingRatio * 2f);

        float targetHalfHeight = paddedHeight / 2f;
        float targetHalfWidth = (paddedWidth / cam.aspect) / 2f;

        // 화면 비율에 맞춰 여백이 포함된 구역이 잘리지 않도록 줌 조절
        cam.orthographicSize = Mathf.Max(targetHalfHeight, targetHalfWidth);

        // 2. 맵 정보 가져오기
        Vector2Int mapSize = mapManager.GetMapSize();
        float mapWidth = mapSize.x;  
        float mapHeight = mapSize.y; 

        // 3. 목표 위치 계산
        Vector3 targetPos = new Vector3(target.position.x, target.position.y, -10f);

        // 4. 💡 맵 경계 제한 (Clamping)
        // 핵심: 카메라 시야가 아무리 넓어져도, 이동 한계선은 오직 '12x8 코어 영역'을 기준으로 막습니다!
        float coreHalfWidth = coreWidth / 2f;
        float coreHalfHeight = coreHeight / 2f;

        float clampedX = CalculateAxisPosition(targetPos.x, mapWidth, coreHalfWidth);
        float clampedY = CalculateAxisPosition(targetPos.y, mapHeight, coreHalfHeight);

        Vector3 desiredPosition = new Vector3(clampedX, clampedY, -10f);

        // 5. 부드러운 이동
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
    }

    private float CalculateAxisPosition(float targetCoord, float mapSpan, float coreHalfSpan)
    {
        float mapMin = -0.5f; 
        float mapMax = mapSpan - 0.5f;

        float limitMin = mapMin + coreHalfSpan;
        float limitMax = mapMax - coreHalfSpan;

        // 코어 영역이 맵의 끝을 넘어가지 않도록 가둠
        return Mathf.Clamp(targetCoord, limitMin, limitMax);
    }
}