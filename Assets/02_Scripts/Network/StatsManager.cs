using System;
using System.Collections;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    private NetworkManager networkManager => NetworkManager.Instance;

    /// <summary>
    /// 게임 결과 업데이트
    /// </summary>
    /// <param name="gameResult">게임 결과</param>
    /// <param name="onSuccess">게임 결과 업데이트 성공 시 실행할 액션</param>
    /// <param name="onFail">게임 결과 업데이트 실패 시 실행할 액션</param>
    public void UpdateGameResult(GameResult gameResult, Action<GameResultResponse> onSuccess, Action<StatsResponseType> onFail)
    {
        StartCoroutine(UpdateGameResultCoroutine(gameResult, onSuccess, onFail));
    }

    private IEnumerator UpdateGameResultCoroutine(GameResult gameResult, Action<GameResultResponse> onSuccess, Action<StatsResponseType> onFail)
    {
        string requestData ="";
        switch (gameResult)
        {
            case GameResult.Victory:
            case GameResult.Disconnect:
                requestData = "win";
                break;
            case GameResult.Defeat:
                requestData = "lose";
                break;
            case GameResult.Draw:
                requestData = "draw";
                break;
        }
        GameResultRequest gameResultRequest = new(requestData);
        yield return networkManager.PostRequestWithSuccess<GameResultRequest, StatsResponse, GameResultResponse>(
            "/stats/updateGameResult",
            gameResultRequest,
            (response) =>
            {
                if (response.connectionResult == NetworkManager.NetworkConnectionResult.NetworkError)
                {
                    Debug.LogError($"네트워크 연결 실패");
                    // TODO: 네트워크 연결 실패
                    return;
                }

                switch (response.data.result)
                {
                    case StatsResponseType.CANNOT_FOUND_USER:
                    case StatsResponseType.INVALID_GAME_RESULT:
                    case StatsResponseType.NOT_LOGGED_IN:
                        onFail?.Invoke(response.data.result);
                        break;
                }
            },
            (result) =>
            {
                onSuccess?.Invoke(result);
            });
    }
    /// <summary>
    /// 사용자 기본 정보 가져오기
    /// </summary>
    /// <param name="onSuccess">사용자 기본 정보 가져오기 성공 시 실행할 액션</param>
    /// <param name="onFail">사용자 기본 정보 가져오기 실패 시 실행할 액션</param>
    public void GetUserInfo(Action<UserInfo_Network> onSuccess, Action<StatsResponseType> onFail)
    {
        StartCoroutine(GetUserInfoCoroutine(onSuccess, onFail));
    }

    private IEnumerator GetUserInfoCoroutine(Action<UserInfo_Network> onSuccess, Action<StatsResponseType> onFail)
    {
        yield return networkManager.SendGetRequest<StatsResponse, UserInfo_Network>("/stats/getUserInfo", (response) =>
            {
                if (response.connectionResult == NetworkManager.NetworkConnectionResult.NetworkError)
                {
                    Debug.LogError($"네트워크 연결 실패");
                    // TODO: 네트워크 연결 실패
                    return;
                }

                switch (response.data.result)
                {
                    case StatsResponseType.NOT_LOGGED_IN:
                        onFail?.Invoke(response.data.result);
                        break;
                }
            },
            (result) => { onSuccess?.Invoke(result); });
    }
    

    /// <summary>
    /// 사용자 전적 가져오기
    /// </summary>
    /// <param name="onSuccess">사용자 전적 가져오기 성공 시 실행할 액션</param>
    /// <param name="onFail">사용자 전적 가져오기 실패 시 실행할 액션</param>
    public void GetRecord(Action<GetRecord> onSuccess, Action<StatsResponseType> onFail)
    {
        StartCoroutine(GetRecordCoroutine(onSuccess, onFail));
    }

    private IEnumerator GetRecordCoroutine(Action<GetRecord> onSuccess, Action<StatsResponseType> onFail)
    {
        yield return networkManager.SendGetRequest<StatsResponse, GetRecord>("/stats/getRecord", (response) =>
            {
                if (response.connectionResult == NetworkManager.NetworkConnectionResult.NetworkError)
                {
                    Debug.LogError($"네트워크 연결 실패");
                    // TODO: 네트워크 연결 실패
                    return;
                }

                switch (response.data.result)
                {
                    case StatsResponseType.NOT_LOGGED_IN:
                        onFail?.Invoke(response.data.result);
                        break;
                }
            },
            (result) => { onSuccess?.Invoke(result); });
    }

    /// <summary>
    /// 전체 유저 랭킹 가져오기
    /// </summary>
    /// <param name="onSuccess">전체 유저 랭킹 가져오기 성공 시 실행할 액션</param>
    /// <param name="onFail">전체 유저 랭킹 가져오기 실패 시 실행할 액션</param>
    public void GetRanking(Action<GetRanking> onSuccess, Action<StatsResponseType> onFail)
    {
        StartCoroutine(GetRankingCoroutine(onSuccess, onFail));
    }

    private IEnumerator GetRankingCoroutine(Action<GetRanking> onSuccess, Action<StatsResponseType> onFail)
    {
        yield return networkManager.SendGetRequest<StatsResponse, GetRanking>("/stats/ranking", (response) =>
            {
                if (response.connectionResult == NetworkManager.NetworkConnectionResult.NetworkError)
                {
                    Debug.LogError($"네트워크 연결 실패");
                    // TODO: 네트워크 연결 실패
                    return;
                }

                switch (response.data.result)
                {
                    case StatsResponseType.CANNOT_FOUND_USER:
                        onFail?.Invoke(response.data.result);
                        break;
                }
            },
            (result) => { onSuccess?.Invoke(result); });
    }
}