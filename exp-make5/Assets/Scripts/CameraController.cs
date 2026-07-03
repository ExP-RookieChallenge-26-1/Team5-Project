using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public Transform target;       // 카메라가 따라다닐 타겟 (플레이어)
    public MapManager mapManager;  // 맵 크기 정보를 가져올 매니저
    public SpriteRenderer backgroundRenderer;
    public Transform viewMask;

    [Header("Movement Settings")]
    public float smoothTime = 0.3f; // 카메라 이동 부드러움
    private Vector3 currentVelocity = Vector3.zero;

    [Header("View Settings")]
    // 💡 화면의 기준이 될 칸 수 
    public float coreWidth = 12f;  // 화면에 절대적으로 보장할 맵의 가로 칸 수
    public float coreHeight = 8f;  // 화면에 절대적으로 보장할 맵의 세로 칸 수

    [Header("Vertical Layout Ratio")]
    public float layoutTopUI = 3f;      // 상단 UI 공간
    public float layoutTopMargin = 1f;  // 맵 위쪽 여백
    public float layoutMap = 15f;       // 맵이 차지할 공간 
    public float layoutBottomMargin = 1f; // 맵 아래쪽 여백


    [Header("UI Padding Settings")]
    [Range(0f, 0.5f)]
    public float paddingRatio = 0.1f;

    [Header("Resolution Settings")]
    public float targetAspectWidth = 20f;  // 💡 목표 가로 비율
    public float targetAspectHeight = 9f;  // 💡 목표 세로 비율

    public SpriteRenderer overlayRenderer;
    public float overlayWidth = 16f;
    public float overlayHeight = 10f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.backgroundColor = Color.black; 

        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.PlayBGM("플레이_BGM.mp3"); 
        }
    }

    void LateUpdate()
    {
        if (target == null || mapManager == null) return;

        // 1. 화면 비율을 20:9로 강제 고정 (레터박스 처리)
        SetFixedAspectRatio();

        // 2. 세로 비율과 가로 너비를 모두 보장하는 줌(Zoom) 계산
        float totalRatio = layoutTopUI + layoutTopMargin + layoutMap + layoutBottomMargin; // 총 20
        
        // 세로 기준으로 필요한 카메라 줌 (15 비율 안에 8칸이 딱 맞아야 함)
        float unitsPerRatioY = coreHeight / layoutMap; 
        float requiredZoomY = (totalRatio * unitsPerRatioY) / 2f;

        // 가로 기준으로 필요한 카메라 줌 (가로 12칸이 무조건 화면에 들어와야 함)
        float requiredZoomX = (coreWidth / cam.aspect) / 2f;

        // 둘 중 더 큰 값을 선택! (가로가 잘릴 상황이면 카메라가 뒤로 물러나며 맵이 비율에 맞게 작아집니다)
        cam.orthographicSize = Mathf.Max(requiredZoomY, requiredZoomX);

        // 3. 카메라가 줌아웃되었더라도, 3:1:15:1 세로 비율을 정확히 유지하기 위해 오프셋 재계산
        float actualTotalHeightUnits = cam.orthographicSize * 2f;
        float actualUnitsPerRatio = actualTotalHeightUnits / totalRatio; 

        float mapCenterRatio = layoutBottomMargin + (layoutMap / 2f); 
        float cameraCenterRatio = totalRatio / 2f;                    
        float offsetRatio = cameraCenterRatio - mapCenterRatio;       
        
        float yOffset = offsetRatio * actualUnitsPerRatio; 

        // 4. 배경 이미지를 변경된 카메라 시야에 완벽히 맞춤
        if (backgroundRenderer != null)
        {
            FitBackgroundToCamera();
        }

        if (overlayRenderer != null)
        {
            FitOverlayToCamera(yOffset);
        }

        if (viewMask != null)
        {
            viewMask.localPosition = new Vector3(0f, -yOffset, 10f); // 카메라는 -10에 있으므로 Z는 10으로 주어 0에 맞춤
            viewMask.localScale = new Vector3(coreWidth, coreHeight, 1f); // 마스크 크기를 무조건 12x8로 강제 고정
        }

        // 5. 맵 정보 가져오기
        Vector2Int mapSize = mapManager.GetMapSize();
        float mapWidth = mapSize.x;  
        float mapHeight = mapSize.y; 

        // 6. 맵 경계 제한 (Clamping)
        Vector3 targetPos = new Vector3(target.position.x, target.position.y, -10f);
        float coreHalfWidth = coreWidth / 2f;
        float coreHalfHeight = coreHeight / 2f;

        float clampedTargetX = CalculateAxisPosition(targetPos.x, mapWidth, coreHalfWidth);
        float clampedTargetY = CalculateAxisPosition(targetPos.y, mapHeight, coreHalfHeight);

        // 최종 위치 = 추적 제한된 타겟 위치 + 레이아웃 오프셋
        Vector3 desiredPosition = new Vector3(clampedTargetX, clampedTargetY + yOffset, -10f);

        // 7. 부드러운 이동
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
    }

    // 종횡비 고정 및 검은 레터박스 생성 함수
    private void SetFixedAspectRatio()
    {
        float targetAspect = targetAspectWidth / targetAspectHeight;
        float currentAspect = (float)Screen.width / Screen.height;
        float scaleHeight = currentAspect / targetAspect;

        Rect rect = cam.rect;

        // 현재 화면이 20:9보다 세로로 길거나 네모난 경우 -> 위아래 검은 띠 (레터박스)
        if (scaleHeight < 1.0f)
        {
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2f;
        }
        // 현재 화면이 20:9보다 가로로 긴 경우 -> 양옆 검은 띠 (필러박스)
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2f;
            rect.y = 0;
        }

        cam.rect = rect;
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

    private void FitBackgroundToCamera()
    {
        if (backgroundRenderer.sprite == null) return;

        // 1. 카메라의 현재 월드 크기 계산 (세로 절반인 orthographicSize * 2 = 전체 세로)
        float cameraHeight = cam.orthographicSize * 2f;
        float cameraWidth = cameraHeight * cam.aspect;

        // 2. 배경 원본 이미지의 픽셀/유닛 크기 가져오기
        float spriteWidth = backgroundRenderer.sprite.bounds.size.x;
        float spriteHeight = backgroundRenderer.sprite.bounds.size.y;

        // 3. 카메라 크기를 원본 이미지 크기로 나누어 정확한 배율(Scale) 도출
        float scaleX = cameraWidth / spriteWidth;
        float scaleY = cameraHeight / spriteHeight;

        // 4. 배경의 스케일과 위치를 강제 고정
        backgroundRenderer.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        // 배경이 항상 카메라 정중앙, 타일보다 뒤쪽(Z: 10)에 있도록 강제 정렬
        backgroundRenderer.transform.localPosition = new Vector3(0f, 0f, 10f); 
    }

    private void FitOverlayToCamera(float yOffset)
    {
        if (overlayRenderer.sprite == null) return;

        // 1. 이미지의 원본 크기(유닛 단위) 가져오기
        float spriteWidth = overlayRenderer.sprite.bounds.size.x;
        float spriteHeight = overlayRenderer.sprite.bounds.size.y;

        // 2. 인스펙터에서 설정한 overlayWidth, overlayHeight 칸 수에 맞게 스케일 계산
        float scaleX = overlayWidth / spriteWidth;
        float scaleY = overlayHeight / spriteHeight;

        overlayRenderer.transform.localScale = new Vector3(scaleX, scaleY, 1f);

        // 3. viewMask와 똑같이 카메라 레이아웃 오프셋을 반영하여 중앙에 고정
        overlayRenderer.transform.localPosition = new Vector3(0.3f, -yOffset, 10f);
    }
}