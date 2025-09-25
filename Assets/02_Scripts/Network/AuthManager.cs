using System;
using System.Collections;
using UnityEngine;

public class AuthManager : MonoBehaviour
{
    private NetworkManager networkManager;

    private void Start()
    {
        networkManager = NetworkManager.Instance;
        if (networkManager == null)
        {
            Debug.LogError("[AuthManager] NetworkManager 인스턴스를 찾을 수 없습니다.");
        }
    }

    #region Public API

    /// <summary>
    /// 회원가입 요청
    /// </summary>
    /// <param name="username">로그인 아이디</param>
    /// <param name="password">비밀번호</param>
    /// <param name="nickname">닉네임</param>
    /// <param name="onSuccess">성공 시 콜백</param>
    /// <param name="onFail">실패 시 콜백</param>
    public void SignUp(string username, string password, string nickname, int profileImage,
        Action onSuccess, Action<AuthResponseType> onFail)
    {
        if (!ValidateRequest("회원가입 요청", onFail)) return;

        StartCoroutine(SignUpCoroutine(username, password, nickname, profileImage, onSuccess, onFail));
    }

    /// <summary>
    /// 로그인 요청
    /// </summary>
    /// <param name="username">로그인 아이디</param>
    /// <param name="password">비밀번호</param>
    /// <param name="onSuccess">성공 시 콜백</param>
    /// <param name="onFail">실패 시 콜백</param>
    public void SignIn(string username, string password, Action onSuccess, Action<AuthResponseType> onFail)
    {
        if (!ValidateRequest("로그인 요청", onFail)) return;

        StartCoroutine(SignInCoroutine(username, password, onSuccess, onFail));
    }

    /// <summary>
    /// 로그아웃 요청
    /// </summary>
    /// <param name="onSuccess">성공 시 콜백</param>
    /// <param name="onFail">실패 시 콜백</param>
    public void SignOut(Action onSuccess, Action<AuthResponseType> onFail)
    {
        if (!ValidateRequest("로그아웃 요청", onFail)) return;

        StartCoroutine(SignOutCoroutine(onSuccess, onFail));
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
            Debug.LogError($"[AuthManager] {requestType} 실패: NetworkManager가 초기화되지 않았습니다.");
            return false;
        }

        return true;
    }

    #endregion

    #region Private Methods - Coroutines

    private IEnumerator SignUpCoroutine(string username, string password, string nickname, int profileImage,
        Action onSuccess, Action<AuthResponseType> onFail)
    {
        SignUpRequest requestData = new(username, password, nickname, profileImage);

        yield return networkManager.PostRequest<SignUpRequest, AuthResponse, AuthResponse>(
            "/auth/signup",
            requestData,
            (response) => HandleAuthError(response, onFail, "회원가입", AuthResponseType.DUPLICATED_USERNAME),
            (response) => onSuccess?.Invoke());
    }


    private IEnumerator SignInCoroutine(string username, string password,
        Action onSuccess, Action<AuthResponseType> onFail)
    {
        SignInRequest requestData = new(username, password);

        yield return networkManager.PostRequest<SignInRequest, AuthResponse, AuthResponse>(
            "/auth/signin",
            requestData,
            (response) => HandleAuthError(response, onFail, "로그인", AuthResponseType.INVALID_PASSWORD,
                AuthResponseType.INVALID_USERNAME),
            (response) => onSuccess?.Invoke());
    }


    private IEnumerator SignOutCoroutine(Action onSuccess, Action<AuthResponseType> onFail)
    {
        yield return networkManager.PostRequest<AuthResponse, AuthResponse>(
            "/auth/signout",
            (response) => HandleAuthError(response, onFail, "로그인", AuthResponseType.NOT_LOGGED_IN),
            (response) => onSuccess?.Invoke());
    }

    #endregion

    #region Private Methods - Helpers

    /// <summary>
    /// Auth 관련 에러 응답 처리
    /// </summary>
    private void HandleAuthError(NetworkManager.NetworkResponse<AuthResponse> response,
        Action<AuthResponseType> onFail, string operationType, params AuthResponseType[] expectedErrors)
    {
        if (response.HasError)
        {
            Debug.LogError($"[AuthManager] {operationType} - 네트워크 연결 실패");
            onFail?.Invoke(AuthResponseType.NETWORK_ERROR);
            return;
        }

        if (response.IsSuccess && ShouldHandleAsError(response.data.result, expectedErrors))
        {
            Debug.LogWarning($"[AuthManager] {operationType} - 서버 에러: {response.data.result}");
            onFail?.Invoke(response.data.result);
        }
    }

    /// <summary>
    /// 에러로 처리해야 하는 응답 타입인지 확인
    /// </summary>
    private bool ShouldHandleAsError(AuthResponseType responseType, params AuthResponseType[] expectedErrors)
    {
        return responseType is not AuthResponseType.SUCCESS;
    }

    #endregion
}