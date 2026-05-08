using System;
using NUnit.Framework;
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
        NotifyAll();
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
        TakeDamage();
        return false;
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

    private void NotifyAll()
    {
        OnHealthChanged?.Invoke(currentHealth);
        OnBlueAmuletChanged?.Invoke(blueAmuletCount);
        OnRedAmuletChanged?.Invoke(redAmuletCount);
    }
}