using UnityEngine;

public class ResolutionFixer209 : MonoBehaviour
{
    void Awake()
    {
        Camera camera = GetComponent<Camera>();
        
        // 목표로 하는 종횡비 (20 : 9)
        float targetAspect = 20f / 9f;

        // 현재 기기/모니터의 실제 화면 종횡비
        float windowAspect = (float)Screen.width / (float)Screen.height;

        // 현재 가로세로 비율을 목표 비율로 나눈 값
        float scaleHeight = windowAspect / targetAspect;

        // 실제 화면이 목표 비율보다 더 뚱뚱한 경우 (예: 16:9 모니터 등)
        if (scaleHeight < 1.0f)
        {
            Rect rect = camera.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            camera.rect = rect;
        }
        // 실제 화면이 목표 비율보다 더 길쭉한 경우
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = camera.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            camera.rect = rect;
        }
    }
}