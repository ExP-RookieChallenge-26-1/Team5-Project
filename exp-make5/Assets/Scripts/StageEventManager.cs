using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// 💡 [수정됨] UI 팀원이 내부 로직을 채워넣을 이벤트 매니저 뼈대
public class StageEventManager : MonoBehaviour
{
    public static StageEventManager Instance;

    [Header("Audio Sources")]
    public AudioClip clearFailedSound;
    public AudioClip clearSuccessSound;
    public AudioClip buttonSound;

    public GameObject gameOverUIPanel;
    public TextMeshProUGUI resultText;

    public bool isGameOver = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        if (gameOverUIPanel != null) gameOverUIPanel.SetActive(false);
    }

    // 💡 [추가됨] 강 접근 시 호출
    public void TriggerRiverEvent()
    {
        Debug.Log("[Event] 강 접근 독백 출력 (UI팀 작업 요망)");
    }

    // 💡 [추가됨] 산 접근 시 호출
    public void TriggerMountainEvent()
    {
        Debug.Log("[Event] 산 접근 독백 출력 (UI팀 작업 요망)");
    }

    // 💡 [추가됨] 향로 조각 획득 시 호출 (몇 번째 조각인지 인자로 받음)
    public void TriggerIncenseFound(int index)
    {
        Debug.Log($"[Event] {index}번째 향로 획득 독백 출력 (UI팀 작업 요망)");
    }

    // 💡 [추가됨] 시간 초과 시 호출
    public void TriggerGameOver_Time()
    {
        if (isGameOver) return;
        isGameOver = true;
        if (SoundManager.Instance != null &&  clearFailedSound!= null) SoundManager.Instance.PlaySFX(clearFailedSound);
        ShowGameOverWindow("시간 초과 게임 오버!");
        VNManager.Instance.StartConversationWithFile("타임 오버 game over 수정1.csv");
    }

    // 💡 [추가됨] 체력 고갈 시 호출
    public void TriggerGameOver_Health()
    {
        if (isGameOver) return;
        isGameOver = true;
        if (SoundManager.Instance != null &&  clearFailedSound!= null) SoundManager.Instance.PlaySFX(clearFailedSound);
        ShowGameOverWindow("체력 고갈 게임 오버!");
        VNManager.Instance.StartConversationWithFile("생명력 소진 game over 수정1.csv");
    }
    
    // 💡 [추가됨] 기권 시 호출
    public void TriggerGameOver_Abstention()
    {
        if (isGameOver) return;
        isGameOver = true;
        if (SoundManager.Instance != null &&  buttonSound!= null) SoundManager.Instance.PlaySFX(buttonSound);
        if (SoundManager.Instance != null &&  clearFailedSound!= null) SoundManager.Instance.PlaySFX(clearFailedSound);
        ShowGameOverWindow("기권 하셨습니다.");
        
    }

    // 💡 [추가됨] 모든 향로 수집 후 수문장 상호작용 시 호출
    public void TriggerStageClear()
    {
        isGameOver = true;
        if (SoundManager.Instance != null &&  clearSuccessSound!= null) SoundManager.Instance.PlaySFX(clearSuccessSound);

        ShowGameOverWindow("성공!");
        
    }

    private void ShowGameOverWindow(string message)
    {
        Debug.Log($"[Event] {message}");
        
        if (gameOverUIPanel != null) gameOverUIPanel.SetActive(true); // 창 켜기
        if (resultText != null) resultText.text = message;            // 사유 적기
    }

    public void RestartGame()
    {
        // 현재 씬의 이름을 가져와서 다시 로드합니다.
        if (SoundManager.Instance != null &&  buttonSound!= null) SoundManager.Instance.PlaySFX(buttonSound);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
    }

    public void StartScene()
    {
        // 현재 씬의 이름을 가져와서 다시 로드합니다.
        if (SoundManager.Instance != null &&  buttonSound!= null) SoundManager.Instance.PlaySFX(buttonSound);
        SceneManager.LoadScene("Start Screen");
    }
}