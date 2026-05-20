using System.Collections.Generic;
using UnityEngine;

// 맵의 모드를 구분하기 위한 열거형(Enum)
public enum StageMode
{
    Casual,
    Story
}

// 유니티 에디터의 우클릭 메뉴에서 이 데이터를 쉽게 생성할 수 있도록 해주는 속성
[CreateAssetMenu(fileName = "New Stage Data", menuName = "Game Data/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("기본 설정 (Common Settings)")]
    public StageMode stageMode = StageMode.Casual; // 캐주얼인지 스토리인지 선택
    public int mapWidth = 18;  // 맵 가로 크기
    public int mapHeight = 14; // 맵 세로 크기
    
    public int maxTime = 100;

    [Header("지뢰 설정 (Mine Settings)")]
    public int leftRedMineCount = 5;  // 빨간 지뢰 개수
    public int leftBlueMineCount = 5; // 파란 지뢰 개수
    public int rightRedMineCount = 5;  // 빨간 지뢰 개수
    public int rightBlueMineCount = 5; // 파란 지뢰 개수

    [Header("지뢰 개수를 양쪽으로 다르게 설정하기 위한 기준")]
    public int divideX = 10;

    [Header("스토리 모드 전용 설정 (Story Mode Only)")]
    [Tooltip("플레이어가 처음 시작할 타일의 위치입니다.")]
    public Vector2Int playerStartPosition;
    public Vector2Int gatekeeperPosition;

    public List<Vector2Int> incensePositions = new List<Vector2Int>();
    public List<Vector2Int> riverPositions = new List<Vector2Int>();
    public List<Vector2Int> mountainPositions = new List<Vector2Int>();
}