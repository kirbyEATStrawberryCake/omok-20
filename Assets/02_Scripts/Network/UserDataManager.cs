using UnityEngine;
using UnityEngine.Events;

public class UserDataManager : MonoBehaviour
{
    public UserInfo_Network UserInfo { get; private set; }
    public UnityAction OnUserInfoChanged;

    /// <summary>
    /// 사용자 정보를 저장하는 메소드
    /// </summary>
    public void SetUserInfo(UnityAction onSuccess = null, UnityAction<StatsResponseType> onFail = null)
    {
        NetworkManager.Instance.statsManager.GetUserInfo(
            (response) =>
            {
                UserInfo = response;
                onSuccess?.Invoke();
                OnUserInfoChanged?.Invoke();
            },
            (errorType) => onFail?.Invoke(errorType));
    }
}