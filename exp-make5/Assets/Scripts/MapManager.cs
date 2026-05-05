using UnityEngine;

// 맵 담당자가 만들어주어야 할 스크립트 (MapManager)
public class MapManager : MonoBehaviour
{
    // 1. 맵 크기 반환 함수
    public Vector2Int GetMapSize()
    {
        // 맵 담당자가 실제 맵 크기를 리턴하도록 구현
        return new Vector2Int(20, 100); 
    }

    // 2. 이동 가능한 타일 위치 배열 (A* 길찾기용)
    // true면 지나갈 수 있는 길(열린 타일이든 닫힌 타일이든), false면 벽/장애물
    public bool[,] GetWalkableGrid()
    {
        // 맵 담당자가 생성된 맵의 장애물 정보를 배열로 리턴하도록 구현
        return new bool[20, 100]; 
    }

    // 3. 타일 상호작용 함수 (캐릭터가 이동 완료 후 호출)
    // 캐릭터(PlayerStatus) 자신을 매개변수로 같이 넘겨주면 맵이 결과를 돌려주기 편합니다.
    public void OpenTile(Vector2Int position, PlayerStatus player)
    {
        // 맵 담당자가 이 좌표(position)에 무엇이 있는지 판별하는 로직 작성
        // 예시 로직:
        /*
        if (지뢰가 없다면) {
            UI에 힌트 숫자 표시 로직;
        } 
        else if (빨간 지뢰라면) {
            player.EncounterRedMine(); // 4. 캐릭터의 함수 실행 (결과 통보)
        } 
        else if (파란 지뢰라면) {
            player.EncounterBlueMine(); // 4. 캐릭터의 함수 실행 (결과 통보)
        }
        */
    }
}