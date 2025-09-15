using System;
using System.Collections;
using UnityEngine;

public class AuthManager : MonoBehaviour
{
    private NetworkManager networkManager => NetworkManager.Instance;

    /// <summary>
    /// 회원가입 요청
    /// </summary>
    /// <param name="username">로그인 아이디</param>
    /// <param name="password">비밀번호</param>
    /// <param name="nickname">닉네임</param>
    /// <param name="onSuccess">회원가입 성공 시 실행할 액션</param>
    /// <param name="onFail">회원가입 실패 시 실행할 액션</param>
    public void SignUp(string username, string password, string nickname,
        Action onSuccess, Action<AuthResponseType> onFail)
    {
        StartCoroutine(SignUpCoroutine(username, password, nickname, onSuccess, onFail));
    }

    private IEnumerator SignUpCoroutine(string username, string password, string nickname,
        Action onSuccess, Action<AuthResponseType> onFail)
    {
        SignUpRequest requestData = new(username, password, nickname);

        yield return networkManager.PostRequest<SignUpRequest, AuthResponse>(
            "/auth/signup",
            requestData,
            response =>
            {
                if (response.connectionResult == NetworkManager.NetworkConnectionResult.NetworkError)
                {
                    Debug.LogError($"네트워크 연결 실패");
                    // TODO: 네트워크 연결 실패
                    return;
                }

                switch (response.data.result)
                {
                    case AuthResponseType.SUCCESS:
                        onSuccess?.Invoke();
                        break;
                    case AuthResponseType.DUPLICATED_USERNAME:
                        onFail?.Invoke(response.data.result);
                        break;
                }
            }
        );
    }


    /// <summary>
    /// 로그인 요청
    /// </summary>
    /// <param name="username">로그인 아이디</param>
    /// <param name="password">비밀번호</param>
    /// <param name="onSuccess">로그인 성공 시 실행할 액션</param>
    /// <param name="onFail">로그인 실패 시 실행할 액션</param>
    public void SignIn(string username, string password, Action onSuccess, Action<AuthResponseType> onFail)
    {
        StartCoroutine(SignInCoroutine(username, password, onSuccess, onFail));
    }

    private IEnumerator SignInCoroutine(string username, string password,
        Action onSuccess, Action<AuthResponseType> onFail)
    {
        SignInRequest requestData = new(username, password);

        yield return networkManager.PostRequest<SignInRequest, AuthResponse>(
            "/auth/signin",
            requestData,
            response =>
            {
                if (response.connectionResult == NetworkManager.NetworkConnectionResult.NetworkError)
                {
                    Debug.LogError($"네트워크 연결 실패");
                    // TODO: 네트워크 연결 실패
                    return;
                }

                switch (response.data.result)
                {
                    case AuthResponseType.SUCCESS:
                        onSuccess?.Invoke();
                        break;
                    case AuthResponseType.INVALID_PASSWORD:
                    case AuthResponseType.INVALID_USERNAME:
                        onFail?.Invoke(response.data.result);
                        break;
                }
            }
        );
    }


    /// <summary>
    /// 로그아웃 요청
    /// </summary>
    /// <param name="onSuccess">로그아웃 성공 시 실행할 액션</param>
    /// <param name="onFail">로그아웃 성공 시 실행할 액션</param>
    public void SignOut(Action onSuccess, Action<AuthResponseType> onFail)
    {
        StartCoroutine(SignOutCoroutine(onSuccess, onFail));
    }

    private IEnumerator SignOutCoroutine(Action onSuccess, Action<AuthResponseType> onFail)
    {
        yield return networkManager.PostRequest<AuthResponse>(
            "/auth/signout",
            response =>
            {
                if (response.connectionResult == NetworkManager.NetworkConnectionResult.NetworkError)
                {
                    Debug.LogError($"네트워크 연결 실패");
                    // TODO: 네트워크 연결 실패
                    return;
                }

                switch (response.data.result)
                {
                    case AuthResponseType.SUCCESS:
                        onSuccess?.Invoke();
                        break;
                    case AuthResponseType.NOT_LOGGED_IN:
                        onFail?.Invoke(response.data.result);
                        break;
                }
            }
        );
    }
}