using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AuthManager))]
[RequireComponent(typeof(StatsManager))]
[RequireComponent(typeof(PointsManager))]
public class NetworkManager : Singleton<NetworkManager>
{
    #region Constants

    public const string ServerURL = "http://localhost:3000";
    public const string SocketServerURL = "ws://localhost:3000";
    private const string EmptyJsonObject = "{}";
    private const string ContentTypeJson = "application/json";

    #endregion


    #region Properties

    public AuthManager authManager { get; private set; }
    public StatsManager statsManager { get; private set; }
    public PointsManager pointsManager { get; private set; }

    #endregion


    #region Enum & Classes

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

        public bool IsSuccess => connectionResult == NetworkConnectionResult.Success;
        public bool HasError => connectionResult == NetworkConnectionResult.NetworkError;
    }

    #endregion

    #region Unity Events

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        InitializeComponents();
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        // Debug.Log($"<color=blue>[NetworkManager] 씬 로드됨: {scene.name}</color>");
    }

    #endregion

    #region Initialization

    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    private void InitializeComponents()
    {
        authManager = GetComponent<AuthManager>();
        statsManager = GetComponent<StatsManager>();
        pointsManager = GetComponent<PointsManager>();

        ValidateComponents();
    }

    /// <summary>
    /// 필수 컴포넌트 유효성 검사
    /// </summary>
    private void ValidateComponents()
    {
        if (authManager == null)
            Debug.LogError("[NetworkManager] AuthManager 컴포넌트를 찾을 수 없습니다.");

        if (statsManager == null)
            Debug.LogError("[NetworkManager] StatsManager 컴포넌트를 찾을 수 없습니다.");

        if (pointsManager == null)
            Debug.LogError("[NetworkManager] PointsManager 컴포넌트를 찾을 수 없습니다.");
    }

    #endregion

    #region Public API - POST Request

    /// <summary>
    /// POST 요청 - 요청 데이터 없음
    /// </summary>
    /// <param name="endpoint">접근 주소</param>
    /// <param name="onError">에러 시 콜백</param>
    /// <param name="onSuccess">성공 시 콜백</param>
    /// <typeparam name="TResponse">에러 응답 타입</typeparam>
    /// <typeparam name="TSuccess">성공 응답 타입</typeparam>
    public IEnumerator PostRequest<TResponse, TSuccess>(string endpoint,
        Action<NetworkResponse<TResponse>> onError, Action<TSuccess> onSuccess)
        where TResponse : class
        where TSuccess : class
    {
        return SendPostRequest<TResponse, TSuccess>(endpoint, EmptyJsonObject, onError, onSuccess);
    }

    /// <summary>
    /// POST 요청 - 요청 데이터 포함
    /// </summary>
    /// <param name="endpoint">접근 주소</param>
    /// <param name="requestData">요청 데이터</param>
    /// <param name="onError">에러 시 콜백</param>
    /// <param name="onSuccess">성공 시 콜백</param>
    /// <typeparam name="TRequest">요청 데이터 타입</typeparam>
    /// <typeparam name="TResponse">에러 응답 타입</typeparam>
    /// <typeparam name="TSuccess">성공 응답 타입</typeparam>
    public IEnumerator PostRequest<TRequest, TResponse, TSuccess>(string endpoint, TRequest requestData,
        Action<NetworkResponse<TResponse>> onError, Action<TSuccess> onSuccess) where TRequest : class
        where TResponse : class
        where TSuccess : class
    {
        string jsonData = JsonUtility.ToJson(requestData);
        return SendPostRequest<TResponse, TSuccess>(endpoint, jsonData, onError, onSuccess);
    }

    #endregion

    #region public API - Get Requests

    /// <summary>
    /// GET 요청 - 성공/실패 콜백 분리
    /// </summary>
    /// <param name="endpoint">접근 주소</param>
    /// <param name="onError">에러 시 콜백</param>
    /// <param name="onSuccess">성공 시 콜백</param>
    /// <typeparam name="TResponse">에러 응답 타입</typeparam>
    /// <typeparam name="TSuccess">성공 응답 타입</typeparam>
    public IEnumerator GetRequest<TResponse, TSuccess>(
        string endpoint,
        Action<NetworkResponse<TResponse>> onError,
        Action<TSuccess> onSuccess)
        where TResponse : class
        where TSuccess : class
    {
        return SendGetRequest<TResponse, TSuccess>(endpoint, onError, onSuccess);
    }

    #endregion

    #region Internal Request Methods - POST

    /// <summary>
    /// 내부 POST 요청 메서드 - 기본형
    /// </summary>
    private IEnumerator SendPostRequest<TResponse>(string endpoint, string jsonData,
        Action<NetworkResponse<TResponse>> onComplete)
        where TResponse : class
    {
        using var request = CreatePostRequest(endpoint, jsonData);
        yield return request.SendWebRequest();

        var response = ProcessResponse<TResponse>(request);
        onComplete?.Invoke(response);
    }

    /// <summary>
    /// 내부 POST 요청 메서드 - 성공/실패 분리형
    /// </summary>
    private IEnumerator SendPostRequest<TResponse, TSuccess>(
        string endpoint,
        string jsonData,
        Action<NetworkResponse<TResponse>> onError,
        Action<TSuccess> onSuccess)
        where TResponse : class
        where TSuccess : class
    {
        using var request = CreatePostRequest(endpoint, jsonData);
        yield return request.SendWebRequest();

        var response = new NetworkResponse<TResponse>();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            response.connectionResult = NetworkConnectionResult.NetworkError;
            onError?.Invoke(response);
        }
        else
        {
            response.connectionResult = NetworkConnectionResult.Success;

            if (request.responseCode >= 200 && request.responseCode < 300)
            {
                // 성공 - TSuccess 타입으로 파싱
                try
                {
                    var result = JsonUtility.FromJson<TSuccess>(request.downloadHandler.text);
                    onSuccess?.Invoke(result);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[NetworkManager] Success JSON 파싱 오류: {ex.Message}");
                }
            }
            else
            {
                // 실패 - TResponse 타입으로 파싱
                try
                {
                    response.data = JsonUtility.FromJson<TResponse>(request.downloadHandler.text);
                    onError?.Invoke(response);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[NetworkManager] Error JSON 파싱 오류: {ex.Message}");
                }
            }
        }
    }

    #endregion

    #region Internal Request Methods - GET

    /// <summary>
    /// 내부 GET 요청 메서드 - 성공/실패 분리형
    /// </summary>
    private IEnumerator SendGetRequest<TResponse, TSuccess>(string endpoint,
        Action<NetworkResponse<TResponse>> onError, Action<TSuccess> onSuccess)
        where TResponse : class where TSuccess : class
    {
        using var request = UnityWebRequest.Get(ServerURL + endpoint);
        yield return request.SendWebRequest();

        ProcessResponse<TResponse, TSuccess>(request, onError, onSuccess);
    }

    #endregion

    #region Request Creation & Response Processing

    /// <summary>
    /// POST 요청 객체 생성
    /// </summary>
    private UnityWebRequest CreatePostRequest(string endpoint, string jsonData)
    {
        var request = new UnityWebRequest(ServerURL + endpoint, UnityWebRequest.kHttpVerbPOST)
        {
            uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData)),
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Content-Type", ContentTypeJson);
        return request;
    }

    /// <summary>
    /// 기본 응답 처리
    /// </summary>
    private NetworkResponse<TResponse> ProcessResponse<TResponse>(UnityWebRequest request)
        where TResponse : class
    {
        var response = new NetworkResponse<TResponse>();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            response.connectionResult = NetworkConnectionResult.NetworkError;
            Debug.LogError($"[NetworkManager] 네트워크 연결 오류: {request.error}");
        }
        else
        {
            response.connectionResult = NetworkConnectionResult.Success;

            if (!string.IsNullOrEmpty(request.downloadHandler.text))
            {
                try
                {
                    response.data = JsonUtility.FromJson<TResponse>(request.downloadHandler.text);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[NetworkManager] JSON 파싱 오류: {ex.Message}");
                }
            }
        }

        return response;
    }

    /// <summary>
    /// 성공/실패 분리 응답 처리
    /// </summary>
    private void ProcessResponse<TResponse, TSuccess>(UnityWebRequest request,
        Action<NetworkResponse<TResponse>> onError, Action<TSuccess> onSuccess) where TSuccess : class
        where TResponse : class
    {
        var response = new NetworkResponse<TResponse>();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            response.connectionResult = NetworkConnectionResult.NetworkError;
            Debug.LogError($"[NetworkManager] 네트워크 연결 오류: {request.error}");
        }
        else
        {
            response.connectionResult = NetworkConnectionResult.Success;

            if (!string.IsNullOrEmpty(request.downloadHandler.text))
            {
                try
                {
                    var result = JsonUtility.FromJson<TSuccess>(request.downloadHandler.text);
                    onSuccess?.Invoke(result);
                }
                catch (System.Exception ex)
                {
                    response.data = JsonUtility.FromJson<TResponse>(request.downloadHandler.text);
                    Debug.LogError($"[NetworkManager] JSON 파싱 오류: {ex.Message}");
                    onError?.Invoke(response);
                }
            }
        }
    }

    #endregion
}