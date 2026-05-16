using System;
using UnityEngine;

// 이 스크립트는 오직 캐릭터의 '상태'만 관리합니다.
public class PlayerStatus : MonoBehaviour
{
    // 프로퍼티를 통해 체력, 빨간 부적, 파란 부적의 수가 변할 때마다 UI측으로 event를 통해 알려줌
    public event Action<int> OnHealthChanged;
    public event Action<int> OnRedAmuletChanged;
    public event Action<int> OnBlueAmuletChanged;

    public event Action<int> OnTimeChanged;

    [SerializeField] private int maxHealth = 3;
    private int currentHealth;
    private int redAmuletCount = 1;
    private int blueAmuletCount = 1;

    private int currentTime;

    public int RedAmuletCount => redAmuletCount; 
    public int BlueAmuletCount => blueAmuletCount;
    public int CurrentHealth => currentHealth;
    public int CurrentTime => currentTime;

    void Start()
    {
        currentHealth = maxHealth;
        NotifyAll();
    }

    // 시작할 때 맵 매니저로부터 최대 시간을 받아옵니다.
    public void InitializeTime(int maxTime)
    {
        currentTime = maxTime;
        OnTimeChanged?.Invoke(currentTime);
    }

    // 이동할 때마다 시간을 소모하는 함수
    public bool UseTime(int amount)
    {
        currentTime -= amount;
        OnTimeChanged?.Invoke(currentTime);
        Debug.Log($"시간 소모: {amount}, 남은 시간: {currentTime}");

        if (currentTime <= 0)
        {
            if (StageEventManager.Instance != null)
            {
                StageEventManager.Instance.TriggerGameOver_Time();
            }
            return false;
        }
        return true;
    }

    public bool HandleMineEncounter(bool isRedMine)
    {
        if (isRedMine) // 빨간 지뢰: 빨간 부적 소모 -> 파란 부적 획득
        {
            if (redAmuletCount > 0) {
                redAmuletCount--; blueAmuletCount++;
                NotifyAll();
                return true;
            }
        }
        else // 파란 지뢰: 파란 부적 소모 -> 빨간 부적 획득
        {
            if (blueAmuletCount > 0) {
                blueAmuletCount--; redAmuletCount++;
                NotifyAll();
                return true;
            }
        }

        // 부적 부족 시 데미지 및 실패 반환
        // 데미지는 코루틴에서 딜레이 연출 이후에 처리할 것이므로 여기서 즉시 깎지 않습니다.
        // TakeDamage(); 
        return false;
    }

    // 외부(코루틴)에서 딜레이 후 호출할 수 있도록 private에서 public으로 변경
    public void TakeDamage() 
    {
        currentHealth--;

        OnHealthChanged?.Invoke(currentHealth);
        Debug.Log($"부적이 부족합니다! 체력 감소. 남은 체력: {currentHealth}");
        
        if (currentHealth <= 0)
        {
            Debug.Log("게임 오버!");
            // 게임 오버 처리 로직
        }
    }

    private void NotifyAll()
    {
        OnHealthChanged?.Invoke(currentHealth);
        OnBlueAmuletChanged?.Invoke(blueAmuletCount);
        OnRedAmuletChanged?.Invoke(redAmuletCount);
        OnTimeChanged?.Invoke(currentTime);
    }
}