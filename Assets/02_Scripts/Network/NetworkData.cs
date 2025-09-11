#region

/// <summary>
/// 유저 식별 정보 저장용 클래스
/// </summary>
[System.Serializable]
public class Identity
{
    public string id; // 고유 식별 id
    public string username; // 유저 로그인 이메일
    public string nickname; // 유저 닉네임
}

/// <summary>
/// 기본 유저 전적 저장용 클래스
/// </summary>
[System.Serializable]
public class Record
{
    public int totalGames; // 총 게임
    public int totalWins; // 총 승리
    public int totalLoses; // 총 패배
    public float winRate; // 승률
}

/// <summary>
/// 포인트 정보 저장용 클래스
/// </summary>
[System.Serializable]
public class Rank
{
    public int points; // 승급 포인트
    public int grade; // 등급
    public bool gradeChanged; // 등급 변경 여부
}

#endregion

#region 급수 및 승급 포인트
/// <summary>
/// 점수 갱신 클래스
/// </summary>
[System.Serializable]
public class SetRank
{
    public string message;
    public Rank rank;
}

/// <summary>
/// 승급 포인트 수신용 클래스
/// </summary>
[System.Serializable]
public class GetPoints
{
    public Identity identity;
    public int points;  // 승급 포인트
}

/// <summary>
/// 등급 수신용 클래스
/// </summary>
[System.Serializable]
public class GetGrade
{
    public Identity identity;
    public int grade;   // 등급
}

#endregion

#region 게임 결과 송수신

/// <summary>
/// 게임 결과 송신용 클래스
/// </summary>
[System.Serializable]
public class GameResultRequest
{
    public string gameResult; // "win" 또는 "lose"
}

/// <summary>
/// 게임 결과 수신용 클래스
/// </summary>
[System.Serializable]
public class GameResultResponse
{
    public string message;
    public string gameResult; // 송신한 게임 결과
    public Record record; 
    public Rank rank;
}

#endregion

#region 전적 수신

/// <summary>
/// 전적 수신용 클래스
/// </summary>
[System.Serializable]
public class GetRecord
{
    public Identity identity;
    public Record record;
}

#endregion

#region 랭킹

/// <summary>
/// 랭킹 수신용 클래스
/// </summary>
[System.Serializable]
public class GetRanking
{
    public RankingUser[] ranking;
}

/// <summary>
/// 랭킹 정보 저장용 클래스
/// </summary>
[System.Serializable]
public class RankingUser
{
    public int rank; // 등수
    public Identity identity;
    public int grade; // 등급
    public Record record;
}

#endregion