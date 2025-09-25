using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GiboUIManager : Singleton<GiboUIManager>
{
    [SerializeField] private GiboBoardManager giboBoardManager;

    [SerializeField] private GameObject FirstButton;
    [SerializeField] private GameObject BeforeButton;
    [SerializeField] private GameObject AfterButton;
    [SerializeField] private GameObject LastButton;

    [Header("화면 UI 설정")]
    [SerializeField] private GameObject gibo;
    [SerializeField] private GameObject popUpGiboExit;
    [SerializeField] private Image leftProfile_Image;
    [SerializeField] private TextMeshProUGUI leftProfile_Grade;
    [SerializeField] private Image rightProfile_Image;
    [SerializeField] private TextMeshProUGUI rightProfile_Grade;

    [Header("프로필 이미지")]
    [SerializeField] private Sprite panda;
    [SerializeField] private Sprite red_panda;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        gibo.SetActive(true);
        FirstButton.GetComponent<Button>().interactable = false;
        BeforeButton.GetComponent<Button>().interactable = false;
        popUpGiboExit.SetActive(false);

    }
    private void OnEnable()
    {
        giboBoardManager.OnButtonChanged += UpdateButtonDisplay;
        giboBoardManager.OnProfileImage += UpdateProfileImage;

    }

    private void OnDisable()
    {

        giboBoardManager.OnButtonChanged -= UpdateButtonDisplay;
        giboBoardManager.OnProfileImage -= UpdateProfileImage;

    }

    /// <summary>
    /// currentIndex가 -1 : 처음
    /// currentIndex가 -2 : 마지막
    /// </summary>
    /// <param name="currentIndex"></param>
    private void UpdateButtonDisplay(int currentIndex) 
    {
        // 기본값: 전부 활성화
        FirstButton.GetComponent<Button>().interactable = true;
        BeforeButton.GetComponent<Button>().interactable = true;
        AfterButton.GetComponent<Button>().interactable = true;
        LastButton.GetComponent<Button>().interactable = true;

        if (currentIndex == -1) // 돌을 두지 않음
        {
            // 맨 처음: First, Before 비활성화
            FirstButton.GetComponent<Button>().interactable = false;
            BeforeButton.GetComponent<Button>().interactable = false;
        }
        else if (currentIndex == -2) // 돌을 다 둠
        {
            // 맨 끝: After, Last 비활성화
            AfterButton.GetComponent<Button>().interactable = false;
            LastButton.GetComponent<Button>().interactable = false;
        }
    }

    /// <summary>
    /// 현재 프로필 UI 업데이트
    /// </summary>
    /// <param name="curRecord"></param>
    private void UpdateProfileImage(GameRecord curRecord) 
    {
        // Todo: 자기거 띄우기
        var userInfo = MultiplayManager.Instance.UserInfo;
        leftProfile_Image.sprite = userInfo.profileImage == 1 ? panda : red_panda;
        leftProfile_Grade.text = curRecord.otherRank == 0 ? $"{userInfo.nickname}": $"{userInfo.grade}급 {userInfo.nickname}";
        // 상대방 정보 띄우기
        rightProfile_Image.sprite = curRecord.otherProfileImage == 1 ? panda :
                             (curRecord.otherProfileImage == 2 ? red_panda : null);
        rightProfile_Grade.text = curRecord.otherRank == 0 ? $"{curRecord.otherPlayerNickname}": $"{curRecord.otherRank}급 {curRecord.otherPlayerNickname}";

    }

}
