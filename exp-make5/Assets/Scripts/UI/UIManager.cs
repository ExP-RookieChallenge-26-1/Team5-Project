using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 추가

public class UIManager : MonoBehaviour
{
    [Header("연결할 캐릭터 스크립트")]
    public PlayerStatus playerStatus;

    [Header("UI 텍스트 요소들")]
    // TMP_Text를 사용하면 UI 텍스트와 3D 텍스트 모두 할당할 수 있습니다.
    public TMP_Text hpText;
    public TMP_Text redAmuletText;
    public TMP_Text blueAmuletText;
    public TMP_Text TimeText;

    void Start()
    {
        // ==========================================
        // 1. 프로퍼티(Getter) 사용: 게임 시작 시 초기 화면 세팅
        // ==========================================
        // playerStatus.CurrentHealth 처럼 괄호 없이 변수처럼 값을 바로 읽어옵니다.
        UpdateHpUI(playerStatus.CurrentHealth);
        UpdateRedAmuletUI(playerStatus.RedAmuletCount);
        UpdateBlueAmuletUI(playerStatus.BlueAmuletCount);
        UpdateTime(playerStatus.CurrentTime);

        // ==========================================
        // 2. 이벤트 구독(Action): 앞으로의 변화 감지
        // ==========================================
        // "앞으로 값이 변할 때마다 이 함수들을 실행시켜줘!" 라고 구독(+=)을 신청합니다.
        playerStatus.OnHealthChanged += UpdateHpUI;
        playerStatus.OnRedAmuletChanged += UpdateRedAmuletUI;
        playerStatus.OnBlueAmuletChanged += UpdateBlueAmuletUI;
        playerStatus.OnTimeChanged += UpdateTime;
    }

    // 캐릭터의 Action이 호출할 실제 화면 갱신 함수들
    private void UpdateHpUI(int hp)
    {
        hpText.text = "" + hp;
    }

    private void UpdateRedAmuletUI(int count)
    {
        redAmuletText.text = "" + count;
    }

    private void UpdateBlueAmuletUI(int count)
    {
        blueAmuletText.text = "" + count;
    }

    private void UpdateTime( int time)
    {
        TimeText.text = "남은 시간 : " + time;
    }

    // ==========================================
    // 3. 이벤트 구독 해제 (매우 중요!)
    // ==========================================
    void OnDestroy()
    {
        // 씬이 넘어가거나 UI가 파괴될 때 반드시 구독을 해제(-=)해야 메모리 누수와 에러를 막을 수 있습니다.
        if (playerStatus != null)
        {
            playerStatus.OnHealthChanged -= UpdateHpUI;
            playerStatus.OnRedAmuletChanged -= UpdateRedAmuletUI;
            playerStatus.OnBlueAmuletChanged -= UpdateBlueAmuletUI;
            playerStatus.OnTimeChanged -= UpdateTime;
        }
    }
}