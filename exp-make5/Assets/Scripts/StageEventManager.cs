using UnityEngine;

// 💡 [수정됨] UI 팀원이 내부 로직을 채워넣을 이벤트 매니저 뼈대
public class StageEventManager : MonoBehaviour
{
    public static StageEventManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 💡 [추가됨] 강 접근 시 호출
    public void TriggerRiverEvent()
    {
        Debug.Log("[Event] 강 접근 독백 출력 (UI팀 작업 요망)");
    }

    // 💡 [추가됨] 향로 조각 획득 시 호출 (몇 번째 조각인지 인자로 받음)
    public void TriggerIncenseFound(int index)
    {
        Debug.Log($"[Event] {index}번째 향로 획득 독백 출력 (UI팀 작업 요망)");
    }

    // 💡 [추가됨] 시간 초과 시 호출
    public void TriggerGameOver_Time()
    {
        Debug.Log("[Event] 시간 초과 게임 오버 연출 (UI팀 작업 요망)");
    }

    // 💡 [추가됨] 체력 고갈 시 호출
    public void TriggerGameOver_Health()
    {
        Debug.Log("[Event] 체력 고갈 게임 오버 연출 (UI팀 작업 요망)");
    }

    // 💡 [추가됨] 모든 향로 수집 후 수문장 상호작용 시 호출
    public void TriggerStageClear()
    {
        Debug.Log("[Event] 스테이지 클리어 (비주얼 노벨 씬 전환 등 UI팀 작업 요망)");
    }
}