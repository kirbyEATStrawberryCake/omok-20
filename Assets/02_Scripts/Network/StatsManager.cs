using System;
using System.Collections;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    private NetworkManager networkManager;

    private void Start()
    {
        networkManager = NetworkManager.Instance;
        if (networkManager == null)
        {
            Debug.LogError("[StatsManager] NetworkManager 인스턴스를 찾을 수 없습니다.");
        }
    }

    #region Public API

    /// <summary>
    /// 게임 결과 업데이트
    /// </summary>
    /// <param name="gameResult">게임 결과</param>
    /// <param name="onSuccess">성공 시 콜백</param>
    /// <param name="onFail">실패 시 콜백</param>
    public void UpdateGameResult(GameResult gameResult, Action<GameResultResponse> onSuccess,
        Action<StatsResponseType> onFail)
    {
        if (!ValidateRequest("게임 결과 업데이트", onFail)) return;

        StartCoroutine(UpdateGameResultCoroutine(gameResult, onSuccess, onFail));
    }

    /// <summary>
    /// 사용자 기본 정보 가져오기
    /// </summary>
    /// <param name="onSuccess">성공 시 콜백</param>
    /// <param name="onFail">실패 시 콜백</param>
    public void GetUserInfo(Action<UserInfo_Network> onSuccess, Action<StatsResponseType> onFail)
    {
        if (!ValidateRequest("사용자 정보 가져오기", onFail)) return;

        StartCoroutine(GetUserInfoCoroutine(onSuccess, onFail));
    }

    /// <summary>
    /// 사용자 전적 가져오기
    /// </summary>
    /// <param name="onSuccess">성공 시 콜백</param>
    /// <param name="onFail">실패 시 콜백</param>
    public void GetRecord(Action<GetRecord> onSuccess, Action<StatsResponseType> onFail)
    {
        if (!ValidateRequest("사용자 전적 가져오기", onFail)) return;

        StartCoroutine(GetRecordCoroutine(onSuccess, onFail));
    }

    /// <summary>
    /// 전체 유저 랭킹 가져오기
    /// </summary>
    /// <param name="onSuccess">성공 시 콜백</param>
    /// <param name="onFail">실패 시 콜백</param>
    public void GetRanking(Action<GetRanking> onSuccess, Action<StatsResponseType> onFail)
    {
        if (!ValidateRequest("전체 유저 랭킹 가져오기", onFail)) return;

        StartCoroutine(GetRankingCoroutine(onSuccess, onFail));
    }

    #endregion

    #region Private Methods - Validation

    /// <summary>
    /// 요청 유효성 검사
    /// </summary>
    private bool ValidateRequest<T>(string requestType, Action<T> onFail) where T : Enum
    {
        if (networkManager == null)
        {
            Debug.LogError($"[StateManager] {requestType} 실패: NetworkManager가 초기화되지 않았습니다.");
            return false;
        }

        return true;
    }

    #endregion

    #region Private Methods - Coroutines

    private IEnumerator UpdateGameResultCoroutine(GameResult gameResult, Action<GameResultResponse> onSuccess,
        Action<StatsResponseType> onFail)
    {
        var gameResultRequest = CreateGameResultRequest(gameResult);

        yield return networkManager.PostRequest<GameResultRequest, StatsResponse, GameResultResponse>(
            "/stats/updateGameResult",
            gameResultRequest,
            (response) => HandleStatsError(response, onFail, "게임 결과 업데이트"),
            onSuccess);
    }

    private IEnumerator GetUserInfoCoroutine(Action<UserInfo_Network> onSuccess, Action<StatsResponseType> onFail)
    {
        yield return networkManager.GetRequest<StatsResponse, UserInfo_Network>(
            "/stats/getUserInfo",
            (response) => HandleStatsError(response, onFail, "사용자 정보 가져오기"),
            onSuccess);
    }

    private IEnumerator GetRecordCoroutine(Action<GetRecord> onSuccess, Action<StatsResponseType> onFail)
    {
        yield return networkManager.GetRequest<StatsResponse, GetRecord>(
            "/stats/getRecord",
            (response) => HandleStatsError(response, onFail, "사용자 전적 가져오기"),
            onSuccess);
    }

    private IEnumerator GetRankingCoroutine(Action<GetRanking> onSuccess, Action<StatsResponseType> onFail)
    {
        yield return networkManager.GetRequest<StatsResponse, GetRanking>(
            "/stats/ranking",
            (response) => HandleStatsError(response, onFail, "전체 유저 랭킹 가져오기"),
            onSuccess);
    }

    #endregion

    #region Private Methods - Helpers

    /// <summary>
    /// 게임 결과를 서버 요청 형식으로 변환
    /// </summary>
    private GameResultRequest CreateGameResultRequest(GameResult gameResult)
    {
        string requestData = gameResult switch
        {
            GameResult.Victory or GameResult.Disconnect => "win",
            GameResult.Defeat => "lose",
            GameResult.Draw => "draw",
            _ => throw new ArgumentOutOfRangeException(nameof(gameResult), gameResult, "지원되지 않는 게임 결과")
        };

        return new GameResultRequest(requestData);
    }

    /// <summary>
    /// Stats 관련 에러 응답 처리
    /// </summary>
    private void HandleStatsError(NetworkManager.NetworkResponse<StatsResponse> response,
        Action<StatsResponseType> onFail, string operationType)
    {
        if (response.HasError)
        {
            Debug.LogError($"[StatsManager] {operationType} - 네트워크 연결 실패");
            return;
        }

        if (response.IsSuccess && ShouldHandleAsError(response.data.result))
        {
            Debug.LogWarning($"[StatsManager] {operationType} - 서버 에러: {response.data.result}");
            onFail?.Invoke(response.data.result);
        }
    }

    /// <summary>
    /// 에러로 처리해야 하는 응답 타입인지 확인
    /// </summary>
    private bool ShouldHandleAsError(StatsResponseType responseType)
    {
        return responseType is
            StatsResponseType.CANNOT_FOUND_USER or
            StatsResponseType.INVALID_GAME_RESULT or
            StatsResponseType.NOT_LOGGED_IN;
    }

    #endregion
}