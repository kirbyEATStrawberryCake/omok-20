using System;
using System.Collections;
using UnityEngine;

public class PointsManager : MonoBehaviour
{
    private NetworkManager networkManager;

    private void Start()
    {
        networkManager = NetworkManager.Instance;
        if (networkManager == null)
        {
            Debug.LogError("[PointsManager] NetworkManager 인스턴스를 찾을 수 없습니다.");
        }
    }

    #region Public API

    /// <summary>
    /// 사용자 포인트 정보 요청
    /// </summary>
    /// <param name="onSuccess">성공 시 콜백</param>
    /// <param name="onFail">실패 시 콜백</param>
    public void GetPoints(Action<GetPoints> onSuccess, Action<PointsResponseType> onFail)
    {
        if (!ValidateRequest("사용자 포인트 정보 요청", onFail)) return;

        StartCoroutine(GetPointsCoroutine(onSuccess, onFail));
    }

    /// <summary>
    /// 사용자 등급 정보 요청
    /// </summary>
    /// <param name="onSuccess">성공 시 콜백</param>
    /// <param name="onFail">실패 시 콜백</param>
    public void GetGrade(Action<GetGrade> onSuccess, Action<PointsResponseType> onFail)
    {
        if (!ValidateRequest("사용자 등급 정보 요청", onFail)) return;

        StartCoroutine(GetGradeCoroutine(onSuccess, onFail));
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
            Debug.LogError($"[PointsManager] {requestType} 실패: NetworkManager가 초기화되지 않았습니다.");
            return false;
        }

        return true;
    }

    #endregion

    #region Private Methods - Coroutines

    private IEnumerator GetPointsCoroutine(Action<GetPoints> onSuccess, Action<PointsResponseType> onFail)
    {
        yield return networkManager.GetRequest<PointsResponse, GetPoints>(
            "/points/getPoints",
            (response) => HandlePointsError(response, onFail, "포인트 가져오기"),
            onSuccess);
    }


    private IEnumerator GetGradeCoroutine(Action<GetGrade> onSuccess, Action<PointsResponseType> onFail)
    {
        yield return networkManager.GetRequest<PointsResponse, GetGrade>(
            "/points/getGrade",
            (response) => HandlePointsError(response, onFail, "등급 가져오기"),
            onSuccess);
    }

    #endregion

    #region Private Methods - Helpers

    private void HandlePointsError(NetworkManager.NetworkResponse<PointsResponse> response,
        Action<PointsResponseType> onFail, string operationType)
    {
        if (response.HasError)
        {
            Debug.LogError($"[PointsManager] {operationType} - 네트워크 연결 실패");
            return;
        }

        if (response.IsSuccess && ShouldHandleAsError(response.data.result))
        {
            Debug.LogWarning($"[PointsManager] {operationType} - 서버 에러: {response.data.result}");
            onFail?.Invoke(response.data.result);
        }
    }

    /// <summary>
    /// 에러로 처리해야 하는 응답 타입인지 확인
    /// </summary>
    private bool ShouldHandleAsError(PointsResponseType responseType)
    {
        return responseType is not PointsResponseType.SUCCESS;
    }

    #endregion
}