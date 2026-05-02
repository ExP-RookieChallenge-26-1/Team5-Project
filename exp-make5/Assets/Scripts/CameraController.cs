using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform target; // 카메라가 따라다닐 타겟 (플레이어)
    public float smoothSpeed = 5f; // 카메라가 따라가는 속도 (클수록 빠름)
    
    // 보여줄 타일 칸 수 (5칸이면 위아래 5칸씩)
    // 화면 끝자락이 살짝 잘리는 것을 방지하기 위해 0.5 정도 여유를 줍니다.
    public float viewSize = 5.5f; 

    void Start()
    {
        // 카메라를 2D(Orthographic) 모드로 확실히 설정하고 뷰 크기를 맞춥니다.
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = viewSize;
    }

    // LateUpdate는 Player의 Update 이동이 모두 끝난 직후에 실행되므로
    // 카메라가 덜덜거리지 않고 부드럽게 따라갑니다.
    void LateUpdate()
    {
        if (target == null) return;

        // 카메라의 목표 위치 계산 (Z축은 카메라 기본값인 -10을 꼭 유지해야 화면에 보입니다)
        Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, -10f);
        
        // Lerp를 이용해 현재 위치에서 목표 위치로 부드럽게 미끄러지듯 이동
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}