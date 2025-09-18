using System;
using System.Collections;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    private NetworkManager networkManager => NetworkManager.Instance;

    /// <summary>
    /// 게임 결과 업데이트
    /// </summary>
    /// <param name="isWin">승리 여부</param>
    /// <param name="onSuccess">게임 결과 업데이트 성공 시 실행할 액션</param>
    /// <param name="onFail">게임 결과 업데이트 실패 시 실행할 액션</param>
    public void UpdateGameResult(bool isWin, Action onSuccess, Action<StatsResponseType> onFail)
    {
        StartCoroutine(UpdateGameResultCoroutine(isWin, onSuccess, onFail));
    }

    private IEnumerator UpdateGameResultCoroutine(bool isWin, Action onSuccess, Action<StatsResponseType> onFail)
    {
        string requestData = isWin ? "win" : "lose";
        GameResultRequest gameResult = new(requestData);
        yield return networkManager.PostRequest<GameResultRequest, StatsResponse>(
            "/stats/updateGameResult",
            gameResult,
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
                    case StatsResponseType.SUCCESS:
                        onSuccess?.Invoke();
                        break;
                    case StatsResponseType.CANNOT_FOUND_USER:
                    case StatsResponseType.INVALID_GAME_RESULT:
                    case StatsResponseType.NOT_LOGGED_IN:
                        onFail?.Invoke(response.data.result);
                        break;
                }
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