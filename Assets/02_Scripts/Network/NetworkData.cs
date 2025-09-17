#region

using System;

#endregion

#region 수신 타입

/// <summary>
/// 인증 응답
/// </summary>
[System.Serializable]
public class AuthResponse
{
    public AuthResponseType result;
}

public enum AuthResponseType
{
    SUCCESS,
    INVALID_USERNAME,
    INVALID_PASSWORD,
    DUPLICATED_USERNAME,
    NOT_LOGGED_IN
}

/// <summary>
/// 포인트 응답
/// </summary>
[System.Serializable]
public class PointsResponse
{
    public PointsResponseType result;
}

public enum PointsResponseType
{
    SUCCESS,
    CANNOT_FOUND_USER,
    INVALID_GAME_RESULT,
    NOT_LOGGED_IN
}

/// <summary>
/// 사용자 전적 응답
/// </summary>
[System.Serializable]
public class StatsResponse
{
    public StatsResponseType result;
}

public enum StatsResponseType
{
    SUCCESS,
    CANNOT_FOUND_USER,
    INVALID_GAME_RESULT,
    NOT_LOGGED_IN
}

#endregion

#region 회원가입, 로그인

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
/// 회원가입 요청
/// </summary>
[System.Serializable]
public class SignUpRequest
{
    public string username;
    public string password;
    public string nickname;
    public int profileImage;

    public SignUpRequest(string username, string password, string nickname, int profileImage)
    {
        this.username = username;
        this.password = password;
        this.nickname = nickname;
        this.profileImage = profileImage;

    }
}

/// <summary>
/// 로그인 요청
/// </summary>
[System.Serializable]
public class SignInRequest
{
    public string username;
    public string password;

    public SignInRequest(string username, string password)
    {
        this.username = username;
        this.password = password;
    }
}

#endregion

#region 급수 및 승급 포인트

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
    public int points; // 승급 포인트
}

/// <summary>
/// 등급 수신용 클래스
/// </summary>
[System.Serializable]
public class GetGrade
{
    public Identity identity;
    public int grade; // 등급
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

    public GameResultRequest(string gameResult)
    {
        this.gameResult = gameResult;
    }
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

/// <summary>
/// 랭킹 수신용 클래스
/// </summary>
[System.Serializable]
public class GetRanking
{
    public RankingUser[] ranking;
}

#endregion

#region 멀티플레이

public enum MultiplayControllerState
{
    CreateRoom, // 방 생성
    JoinRoom,   // 생성된 방 참여
    ExitRoom,   // 클라이언트가 방에서 나갈 때
    OpponentJoined, // 상대방이 들어왔을 때
    OpponentLeft   // 상대방이 접속을 끊거나 방을 나갔을 때
}

#endregion