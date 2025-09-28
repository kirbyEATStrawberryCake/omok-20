using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ProfileImageType
{
    Panda = 1,
    RedPanda = 2
}

public class PlayerProfileUIController : MonoBehaviour
{
    [Header("플레이어 프로필")]
    [SerializeField] private Image leftProfileImage;
    [SerializeField] private Image rightProfileImage;
    [SerializeField] private TMP_Text player1GradeAndNickname;
    [SerializeField] private TMP_Text player2GradeAndNickname;

    [Header("프로필용 스프라이트")]
    [SerializeField] private Sprite pandaSprite;
    [SerializeField] private Sprite redPandaSprite;
    [SerializeField] private Sprite winPandaProfileSprite;
    [SerializeField] private Sprite losePandaProfileSprite;
    [SerializeField] private Sprite winRedPandaProfileSprite;
    [SerializeField] private Sprite loseRedPandaProfileSprite;

    private MultiplayManager multiplayManager;
    // TODO: 기보매니저 처리하기
    // private GiboManager giboManager;
    private UserInfo_Network userInfo;
    private MatchData opponentData;

    #region Unity Life Cycle

    private void Start()
    {
        multiplayManager = MultiplayManager.Instance;
        // giboManager = GiboManager.Instance;
        userInfo = NetworkManager.Instance.userDataManager.UserInfo;
    }

    #endregion

    #region 프로필 설정

    /// <summary>
    /// 싱글플레이 시 사용자 프로필을 설정
    /// </summary>
    public void UpdatePlayerProfileInSinglePlay()
    {
        leftProfileImage.sprite = pandaSprite;
        rightProfileImage.sprite = redPandaSprite;
    }

    /// <summary>
    /// 멀티플레이 시 사용자 프로필을 가져옴
    /// </summary>
    public void UpdatePlayerProfileInMultiPlay()
    {
        if (GameModeManager.Mode != GameMode.MultiPlayer && GameModeManager.Mode != GameMode.AI) return;

        // 나의 프로필은 기기에 있는 정보 사용
        leftProfileImage.sprite = GetProfileSprite((ProfileImageType)userInfo.profileImage);
        player1GradeAndNickname.text = $"{userInfo.grade}급 {userInfo.nickname}";
        // 상대의 프로필은 서버에서 받아온 정보를 사용하여 프로필 업데이트
        if (GameModeManager.Mode == GameMode.MultiPlayer)
        {
            opponentData = multiplayManager.MatchData;
            rightProfileImage.sprite = GetProfileSprite((ProfileImageType)opponentData.profileImage);
            player2GradeAndNickname.text = $"{opponentData.grade}급 {opponentData.nickname}";
            // giboManager.SetGiboProfileData(opponentData.nickname, opponentData.profileImage, opponentData.grade);
        }
        else if (GameModeManager.Mode == GameMode.AI)
        {
            rightProfileImage.sprite = GetProfileSprite((ProfileImageType)userInfo.profileImage, true);
            // TODO: AI 랜덤이름? -> 일단 값은 고정적으로 세팅했습니다!
            // giboManager.SetGiboProfileData("AI1", 2, 10);
        }
    }

    #endregion

    #region 게임 종료 후 프로필 설정

    /// <summary>
    /// 오목 결과에 따라서 프로필 사진을 변경하는 메소드(싱글)
    /// </summary>
    /// <param name="result"></param>
    public void UpdateProfileImagesOnResultInSinglePlay(GameResult result)
    {
        if (leftProfileImage == null || rightProfileImage == null)
        {
            Debug.LogWarning("Profile images not assigned!");
            return;
        }

        switch (result)
        {
            case GameResult.Player1Win:
                leftProfileImage.sprite = winPandaProfileSprite;
                rightProfileImage.sprite = loseRedPandaProfileSprite;
                break;
            case GameResult.Player2Win:
                leftProfileImage.sprite = losePandaProfileSprite;
                rightProfileImage.sprite = winRedPandaProfileSprite;
                break;
            case GameResult.Draw:
                leftProfileImage.sprite = winPandaProfileSprite;
                rightProfileImage.sprite = winRedPandaProfileSprite;
                break;
            default:
                Debug.Log("No profile update for result: " + result);
                break;
        }
    }

    /// <summary>
    /// 오목 결과에 따라서 프로필 사진을 변경하는 메소드(멀티)
    /// </summary>
    /// <param name="result"></param>
    public void UpdateProfileImagesOnResultInMultiplay(GameResult result)
    {
        if (leftProfileImage == null || rightProfileImage == null)
        {
            Debug.LogWarning("Profile images not assigned!");
            return;
        }

        switch (result)
        {
            case GameResult.Victory: // 내가 승리 (멀티)
                leftProfileImage.sprite = GetResultSprite((ProfileImageType)userInfo.profileImage, true);
                if (GameModeManager.Mode == GameMode.MultiPlayer)
                    rightProfileImage.sprite = GetResultSprite((ProfileImageType)opponentData.profileImage, false);
                else if (GameModeManager.Mode == GameMode.AI)
                    rightProfileImage.sprite = GetResultSprite((ProfileImageType)userInfo.profileImage, false, true);
                break;
            case GameResult.Defeat: // 내가 패배 (멀티)
                leftProfileImage.sprite = GetResultSprite((ProfileImageType)userInfo.profileImage, false);
                if (GameModeManager.Mode == GameMode.MultiPlayer)
                    rightProfileImage.sprite = GetResultSprite((ProfileImageType)opponentData.profileImage, true);
                else if (GameModeManager.Mode == GameMode.AI)
                    rightProfileImage.sprite = GetResultSprite((ProfileImageType)userInfo.profileImage, true, true);
                break;
            case GameResult.Draw:
                leftProfileImage.sprite = winPandaProfileSprite;
                rightProfileImage.sprite = winRedPandaProfileSprite;
                break;
            default:
                Debug.Log("No profile update for result: " + result);
                break;
        }
    }

    #endregion


    #region Private Methods

    /// <summary>
    /// 사용자 프로필 스프라이트를 가져옴
    /// </summary>
    /// <param name="type">프로필 이미지 타입</param>
    /// <param name="isAI">AI인가?(기본 false)</param>
    /// <returns></returns>
    private Sprite GetProfileSprite(ProfileImageType type, bool isAI = false)
    {
        if (isAI)
            return type == ProfileImageType.Panda ? redPandaSprite : pandaSprite;
        else
            return type == ProfileImageType.Panda ? pandaSprite : redPandaSprite;
    }

    /// <summary>
    /// 게임 종료 시 사용자 프로필 스프라이트를 가져옴
    /// </summary>
    /// <param name="type">프로필 이미지 타입</param>
    /// <param name="isWin">승리했는가?</param>
    /// <param name="isAI">AI인가?(기본 false)</param>
    /// <returns></returns>
    private Sprite GetResultSprite(ProfileImageType type, bool isWin, bool isAI = false)
    {
        if (isAI)
            type = (type == ProfileImageType.Panda) ? ProfileImageType.RedPanda : ProfileImageType.Panda;

        if (isWin)
            return type == ProfileImageType.Panda ? winPandaProfileSprite : winRedPandaProfileSprite;
        else
            return type == ProfileImageType.Panda ? losePandaProfileSprite : loseRedPandaProfileSprite;
    }

    #endregion
}