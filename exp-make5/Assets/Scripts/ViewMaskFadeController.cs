using UnityEngine;

// 시야 가장자리를 부드럽게 어둡게(희미하게) 만드는 오버레이 컨트롤러.
// 카메라 뷰를 가득 덮는 Quad에 Custom/ViewMaskFade 셰이더 머티리얼을 입혀 사용합니다.
[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer))]
public class ViewMaskFadeController : MonoBehaviour
{
    [Header("References")]
    public Camera targetCamera;     // 비우면 Camera.main 사용
    public Transform viewMask;      // 시야 중심/크기 기준 (CameraController.viewMask)

    [Header("Fade Settings")]
    [Tooltip("viewMask 크기 대비 '완전히 선명한' 안쪽 영역 비율 (1 = viewMask와 동일 크기)")]
    [Range(0.1f, 1.5f)] public float clearScale = 0.85f;

    [Tooltip("선명한 영역에서 완전히 어두워질 때까지의 페이드 폭 (UV 기준)")]
    [Range(0.01f, 1f)] public float softness = 0.25f;

    [Tooltip("바깥 영역을 덮을 색 (보통 검정). 알파로 최대 어두움 정도 조절")]
    public Color overlayColor = Color.black;

    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock mpb;

    void OnEnable()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        mpb = new MaterialPropertyBlock();
        if (targetCamera == null) targetCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (targetCamera == null || !targetCamera.orthographic) return;
        if (meshRenderer == null || mpb == null) OnEnable();

        float orthoH = targetCamera.orthographicSize * 2f;
        float orthoW = orthoH * targetCamera.aspect;

        // 1. 카메라 뷰를 정확히 가득 채우도록 Quad를 카메라 바로 앞에 배치
        transform.position = targetCamera.transform.position
                             + targetCamera.transform.forward * (targetCamera.nearClipPlane + 0.05f);
        transform.rotation = targetCamera.transform.rotation;
        transform.localScale = new Vector3(orthoW, orthoH, 1f);

        // 2. 구멍 중심과 반경 계산
        Vector2 center = new Vector2(0.5f, 0.5f);
        float innerRadius = 0.3f;

        if (viewMask != null)
        {
            Vector3 vp = targetCamera.WorldToViewportPoint(viewMask.position);
            center = new Vector2(vp.x, vp.y);

            // viewMask 세로 절반 크기를 뷰포트(0~1) 비율로 환산
            float halfH = viewMask.lossyScale.y * 0.5f;
            innerRadius = (halfH / orthoH) * clearScale;
        }

        // 3. 셰이더 프로퍼티 전달 (MaterialPropertyBlock 사용 - 머티리얼 인스턴스 누수 방지)
        meshRenderer.GetPropertyBlock(mpb);
        mpb.SetColor("_Color", overlayColor);
        mpb.SetVector("_Center", new Vector4(center.x, center.y, 0f, 0f));
        mpb.SetFloat("_InnerRadius", innerRadius);
        mpb.SetFloat("_OuterRadius", innerRadius + softness);
        mpb.SetFloat("_Aspect", targetCamera.aspect);
        meshRenderer.SetPropertyBlock(mpb);
    }
}
