using System;
using UnityEngine;

// 이 스크립트는 오직 캐릭터의 '상태'만 관리합니다.
public class PlayerStatus : MonoBehaviour
{
    // 프로퍼티를 통해 체력, 빨간 부적, 파란 부적의 수가 변할 때마다 UI측으로 event를 통해 알려줌
    public event Action<int> OnHealthChanged;
    public event Action<int> OnRedAmuletChanged;
    public event Action<int> OnBlueAmuletChanged;

    [SerializeField] private int maxHealth = 3;
    private int currentHealth;
    
    private int redAmuletCount = 1;
    private int blueAmuletCount = 1;

    public int RedAmuletCount => redAmuletCount; 
    public int BlueAmuletCount => blueAmuletCount;
    public int CurrentHealth => currentHealth;

    void Start()
    {
        currentHealth = maxHealth;

        OnHealthChanged?.Invoke(currentHealth);
    }

    // 빨간 몬스터를 만났을 때 맵 매니저가 호출할 함수
    public void EncounterRedMine()
    {
        if (redAmuletCount > 0)
        {
            redAmuletCount--;
            blueAmuletCount++;

            OnRedAmuletChanged?.Invoke(redAmuletCount);
            OnBlueAmuletChanged?.Invoke(blueAmuletCount);
            Debug.Log("빨간 몬스터 처치! 빨간 부적 소비 -> 파란 부적 획득");
        }
        else
        {
            TakeDamage();
        }
    }

    // 파란 몬스터를 만났을 때 맵 매니저가 호출할 함수
    public void EncounterBlueMine()
    {
        if (blueAmuletCount > 0)
        {
            blueAmuletCount--;
            redAmuletCount++;

            OnBlueAmuletChanged?.Invoke(blueAmuletCount);
            OnRedAmuletChanged?.Invoke(redAmuletCount);
            Debug.Log("파란 몬스터 처치! 파란 부적 소비 -> 빨간 부적 획득");
        }
        else
        {
            TakeDamage();
        }
    }

    private void TakeDamage()
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
}