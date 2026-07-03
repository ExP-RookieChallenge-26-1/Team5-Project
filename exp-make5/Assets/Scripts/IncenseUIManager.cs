using UnityEngine;

public class IncenseUIManager : MonoBehaviour
{
    public static IncenseUIManager Instance { get; private set; }

    [Header("UI 슬롯 설정 (0번 칸 ~ 3번 칸 순서대로 매칭)")]
    // 각 네모 칸의 자식으로 넣을 블러 이미지와 선명한 이미지 오브젝트들
    public GameObject[] blurredUIElements = new GameObject[4]; // Element 0: 조각1 블러, Element 1: 조각2 블러...
    public GameObject[] realUIElements = new GameObject[4];    // Element 0: 조각1 선명, Element 1: 조각2 선명...

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ResetUI();
    }

    public void ResetUI()
    {
        // 게임 시작 시 모든 칸은 블러 이미지만 켜고, 선명한 이미지는 꺼둡니다.
        for (int i = 0; i < 4; i++)
        {
            if (blurredUIElements[i] != null) blurredUIElements[i].SetActive(true);
            if (realUIElements[i] != null) realUIElements[i].SetActive(false);
        }
    }

    /// <summary>
    /// 맵에서 획득한 향로의 고유 번호(index)를 받아와 해당 UI 칸을 선명하게 변경합니다.
    /// </summary>
    public void CollectIncense(int index)
    {
        if (index < 0 || index >= 4) return;

        // 전달받은 고유 번호와 일치하는 배열 원소의 블러를 끄고 선명한 이미지를 켭니다.
        if (blurredUIElements[index] != null) blurredUIElements[index].SetActive(false);
        if (realUIElements[index] != null) realUIElements[index].SetActive(true);
    }
}