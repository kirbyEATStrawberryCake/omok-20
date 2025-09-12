using System;
using System.Collections;
using UnityEngine;

public class PointsManager : MonoBehaviour
{
    private NetworkManager networkManager => NetworkManager.Instance;

    public void GetPoints(Action<GetPoints> onSuccess, Action<PointsResponseType> onFail)
    {
        StartCoroutine(GetPointsCoroutine(onSuccess, onFail));
    }

    private IEnumerator GetPointsCoroutine(Action<GetPoints> onSuccess, Action<PointsResponseType> onFail)
    {
        yield return networkManager.SendGetRequest<PointsResponse, GetPoints>("/points/getPoints", (response) =>
            {
                if (response.connectionResult == NetworkManager.NetworkConnectionResult.NetworkError)
                {
                    Debug.LogError($"네트워크 연결 실패");
                    // TODO: 네트워크 연결 실패
                    return;
                }

                switch (response.data.result)
                {
                    case PointsResponseType.NOT_LOGGED_IN:
                        onFail?.Invoke(response.data.result);
                        break;
                }
            },
            (result) => onSuccess?.Invoke(result));
    }

    public void GetGrade(Action<GetGrade> onSuccess, Action<PointsResponseType> onFail)
    {
        StartCoroutine(GetGradeCoroutine(onSuccess, onFail));
    }

    private IEnumerator GetGradeCoroutine(Action<GetGrade> onSuccess, Action<PointsResponseType> onFail)
    {
        yield return networkManager.SendGetRequest<PointsResponse, GetGrade>("/points/getGrade", (response) =>
            {
                if (response.connectionResult == NetworkManager.NetworkConnectionResult.NetworkError)
                {
                    Debug.LogError($"네트워크 연결 실패");
                    // TODO: 네트워크 연결 실패
                    return;
                }

                switch (response.data.result)
                {
                    case PointsResponseType.NOT_LOGGED_IN:
                        onFail?.Invoke(response.data.result);
                        break;
                }
            },
            (result) => onSuccess?.Invoke(result));
    }
}