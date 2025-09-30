using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GiboUIManager : MonoBehaviour
{
    [SerializeField] private GameObject FirstButton;
    [SerializeField] private GameObject BeforeButton;
    [SerializeField] private GameObject AfterButton;
    [SerializeField] private GameObject LastButton;

    [Header("화면 UI 설정")]
    [SerializeField] private Image leftProfile_Image;
    [SerializeField] private TMP_Text leftProfile_Grade;
    [SerializeField] private Image rightProfile_Image;
    [SerializeField] private TMP_Text rightProfile_Grade;

    [Header("프로필 이미지")]
    [SerializeField] private Sprite panda;
    [SerializeField] private Sprite red_panda;

    private void Start()
    {
        FirstButton.GetComponent<Button>().interactable = false;
        BeforeButton.GetComponent<Button>().interactable = false;
    }

    /// <summary>
    /// currentIndex가 -1 : 처음
    /// currentIndex가 -2 : 마지막
    /// </summary>
    public void UpdateButtonDisplay(int currentIndex)
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
    public void UpdateProfileImage(GameRecord curRecord)
    {
        // 자기거 띄우기
        var userInfo = NetworkManager.Instance.userDataManager.UserInfo;
        leftProfile_Image.sprite = userInfo.profileImage == 1 ? panda : red_panda;
        leftProfile_Grade.text = curRecord.otherRank == 0 ? $"{userInfo.nickname}" : $"{userInfo.grade}급 {userInfo.nickname}";
        // 상대방 정보 띄우기
        rightProfile_Image.sprite =
            curRecord.otherProfileImage == 1 ? panda : (curRecord.otherProfileImage == 2 ? red_panda : null);
        rightProfile_Grade.text = curRecord.otherRank == 0
            ? $"{curRecord.otherPlayerNickname}"
            : $"{curRecord.otherRank}급 {curRecord.otherPlayerNickname}";
    }
}