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

    // 12지(支) 시진 글자 (자시부터 시작). "시"는 별도로 붙입니다.
    private static readonly string[] Jiji =
    {
        "자", "축", "인", "묘", "진", "사",
        "오", "미", "신", "유", "술", "해"
    };

    [Header("시간 표기 폰트 크기")]
    [Tooltip("시진 글자(자, 축…)와 각 숫자(1, 2… / 정)를 일반 글자(시, 각) 대비 몇 %로 키울지")]
    public float emphasisSizePercent = 150f;

    private void UpdateTime( int time)
    {
        TimeText.text = FormatTime(time);
    }

    // 내부 시간 값을 전통 시간 표기(시·각)로 변환합니다.
    // 1시 = 8각, 자시 정각에서 시작하여 시간이 소모될수록 진행되며,
    // 모두 소모(시간 0)되면 묘시 정각이 됩니다. (자→축→인→묘, 총 24각 = 360분)
    // 시진 글자와 각 숫자(또는 '정')는 <size> 리치 텍스트로 크게 표시합니다.
    private string FormatTime(int time)
    {
        int max = playerStatus.MaxTime;
        if (max <= 0) max = 1;

        const int totalGak = 24; // 자시 정각 ~ 묘시 정각 (3시진 × 8각)

        int elapsed = max - time;
        if (elapsed < 0) elapsed = 0;

        // 소모한 비율만큼 각(刻)이 진행됩니다.
        int gakPassed = Mathf.Clamp(Mathf.FloorToInt((float)elapsed / max * totalGak), 0, totalGak);

        int siIndex = Mathf.Min(gakPassed / 8, Jiji.Length - 1);
        int gak = gakPassed % 8;

        string si = Big(Jiji[siIndex]);              // 큰 글자: 시진
        string gakValue = Big(gak == 0 ? "정" : gak.ToString()); // 큰 글자: '정' 또는 숫자
        return $"{si}시 {gakValue}각";
    }

    // 글자를 강조 크기로 감싸는 리치 텍스트 헬퍼
    private string Big(string text)
    {
        return $"<size={emphasisSizePercent:0}%>{text}</size>";
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