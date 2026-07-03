using UnityEngine;
using UnityEngine.UI; // UI 컴포넌트를 사용하기 위해 필요

public class PlayerTimeUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayerStatus playerStatus; // 연동할 PlayerStatus 스크립트
    [SerializeField] private Slider timeSlider;         // UI의 Slider 컴포넌트

    private void OnEnable()
    {
        if (playerStatus != null)
        {
            // 시간이 변경될 때마다 UpdateTimeUI 함수가 실행되도록 이벤트 구독
            playerStatus.OnTimeChanged += UpdateTimeUI;
        }
    }

    private void OnDisable()
    {
        if (playerStatus != null)
        {
            // 메모리 누수 방지를 위해 이벤트 구독 해제
            playerStatus.OnTimeChanged -= UpdateTimeUI;
        }
    }

    private void UpdateTimeUI(int currentTime)
    {
        if (timeSlider == null || playerStatus == null) return;

        // 슬라이더의 최대값을 PlayerStatus의 maxTime으로 설정
        timeSlider.maxValue = playerStatus.MaxTime;

        // 슬라이더의 현재 가치(Value)를 currentTime으로 설정
        timeSlider.value = currentTime;
    }
}