using System.Linq;

public class GiboBoardManager : BoardManager
{
    // 기보용
    private GameRecord currentRecord;

    private void Awake() { InitializeBoard(); }

    /// <summary>
    /// 사용할 기보 설정
    /// </summary>
    public void SetRecord(GameRecord record) { currentRecord = record; }

    #region 버튼 이벤트 (처음 / 이전 / 다음 / 끝)

    public void ShowFirst()
    {
        ClearBoard();
        HideAllMarkers();
    }

    public void ShowPrevious(int index)
    {
        // 마지막 수 제거
        var lastStone = stoneObjects.Last();
        stoneObjects.Remove(lastStone);
        Destroy(lastStone.gameObject);

        if (index == -1)
            HideAllMarkers();
        else
        {
            MoveData prevMove = currentRecord.moves[index];
            UpdateLastMoveMarker(prevMove.x, prevMove.y);
        }
    }

    public void ShowNext(int index)
    {
        MoveData move = currentRecord.moves[index];
        PlaceStoneVisual(move.x, move.y, move.stoneColor == 1 ? StoneType.Black : StoneType.White);
        UpdateLastMoveMarker(move.x, move.y);
    }

    public void ShowLast()
    {
        // 보드 초기화 후 모든 돌 배치
        ClearBoard();
        foreach (MoveData move in currentRecord.moves)
        {
            PlaceStoneVisual(move.x, move.y, move.stoneColor == 1 ? StoneType.Black : StoneType.White);
        }

        var lastMove = currentRecord.moves.Last();
        UpdateLastMoveMarker(lastMove.x, lastMove.y);
    }

    #endregion
}