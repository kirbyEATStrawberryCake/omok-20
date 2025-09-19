using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class NetworkManager : Singleton<NetworkManager>
{
    public const string ServerURL = "http://localhost:3000";
    public const string SocketServerURL = "ws://localhost:3000";

    /// <summary>
    /// 네트워크 연결 결과
    /// </summary>
    public enum NetworkConnectionResult
    {
        Success, // 서버와 통신 성공 (응답 받음)
        NetworkError // 네트워크 연결 실패
    }

    /// <summary>
    /// 네트워크 응답 (연결 상태 + 서버 응답 데이터)
    /// </summary>
    public class NetworkResponse<T> where T : class
    {
        public NetworkConnectionResult connectionResult;
        public T data;
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        // 씬 로드 공통 처리
    }

    /// <summary>
    /// POST 요청
    /// </summary>
    /// <param name="endpoint">접근 주소</param>
    /// <param name="requestData">송신 데이터</param>
    /// <param name="onComplete">네트워크 처리 완료 시 실행할 액션</param>
    /// <typeparam name="TRequest">송신 타입</typeparam>
    /// <typeparam name="TResponse">수신 타입</typeparam>
    /// <returns></returns>
    public IEnumerator PostRequest<TRequest, TResponse>(string endpoint, TRequest requestData,
        Action<NetworkResponse<TResponse>> onComplete)
        where TRequest : class
        where TResponse : class
    {
        string jsonData = JsonUtility.ToJson(requestData);
        return SendPostRequest<TResponse>(endpoint, jsonData, onComplete);
    }

    /// <summary>
    /// POST 요청(Request 데이터 없음)
    /// </summary>
    /// <param name="endpoint">접근 주소</param>
    /// <param name="onComplete">네트워크 처리 완료 시 실행할 액션</param>
    /// <typeparam name="TResponse">수신 타입</typeparam>
    /// <returns></returns>
    public IEnumerator PostRequest<TResponse>(string endpoint,
        Action<NetworkResponse<TResponse>> onComplete)
        where TResponse : class
    {
        string emptyJson = "{}"; // 빈 JSON 객체
        return SendPostRequest<TResponse>(endpoint, emptyJson, onComplete);
    }

    public IEnumerator PostRequestWithSuccess<TRequest, TResponse, TSuccess>(string endpoint, TRequest requestData,
        Action<NetworkResponse<TResponse>> onComplete, Action<TSuccess> onSuccess) where TRequest : class
        where TResponse : class
        where TSuccess : class
    {
        string jsonData = JsonUtility.ToJson(requestData);
        using UnityWebRequest www = new UnityWebRequest(ServerURL + endpoint, UnityWebRequest.kHttpVerbPOST);
        www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        NetworkResponse<TResponse> response = new NetworkResponse<TResponse>();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            // 네트워크 연결 실패
            response.connectionResult = NetworkConnectionResult.NetworkError;
            onComplete?.Invoke(response);
        }
        else
        {
            response.connectionResult = NetworkConnectionResult.Success;
            if (www.responseCode == 200)
            {
                // 성공 시 데이터 파싱하여 onSuccess 호출
                var result = JsonUtility.FromJson<TSuccess>(www.downloadHandler.text);
                onSuccess?.Invoke(result);
            }
            else
            {
                // 실패 시 에러 응답 파싱
                response.data = JsonUtility.FromJson<TResponse>(www.downloadHandler.text);
                onComplete?.Invoke(response);
            }
        }
    }

    /// <summary>
    /// 공통 POST 요청 메서드
    /// </summary>
    public IEnumerator SendPostRequest<TResponse>(string endpoint, string jsonData,
        Action<NetworkResponse<TResponse>> onComplete)
        where TResponse : class
    {
        using UnityWebRequest www = new UnityWebRequest(ServerURL + endpoint, UnityWebRequest.kHttpVerbPOST);
        www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        NetworkResponse<TResponse> response = new NetworkResponse<TResponse>();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            // 네트워크 연결 실패
            response.connectionResult = NetworkConnectionResult.NetworkError;
        }
        else
        {
            // 서버와 통신 성공
            response.connectionResult = NetworkConnectionResult.Success;
            response.data = JsonUtility.FromJson<TResponse>(www.downloadHandler.text);
        }

        onComplete?.Invoke(response);
    }

    /// <summary>
    /// 공통 GET 요청 메서드
    /// </summary>
    public IEnumerator SendGetRequest<TResponse, TSuccess>(string endpoint,
        Action<NetworkResponse<TResponse>> onComplete, Action<TSuccess> onSuccess)
        where TResponse : class
    {
        using UnityWebRequest www = UnityWebRequest.Get(ServerURL + endpoint);

        yield return www.SendWebRequest();

        NetworkResponse<TResponse> response = new NetworkResponse<TResponse>();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            // 네트워크 연결 실패
            response.connectionResult = NetworkConnectionResult.NetworkError;
            onComplete?.Invoke(response);
        }
        else
        {
            // 서버와 통신 성공
            response.connectionResult = NetworkConnectionResult.Success;
            if (www.responseCode == 200)
            {
                // 데이터 수신 성공
                var result = JsonUtility.FromJson<TSuccess>(www.downloadHandler.text);
                onSuccess?.Invoke(result);
            }
            else
            {
                // 데이터 수신 실패
                response.data = JsonUtility.FromJson<TResponse>(www.downloadHandler.text);
                onComplete?.Invoke(response);
            }
        }
    }
}