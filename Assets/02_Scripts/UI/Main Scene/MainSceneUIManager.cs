using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainSceneUIManager : Singleton<MainSceneUIManager>
{
    [Header("유저 정보")]
    [SerializeField] [Tooltip("프로필 오브젝트")] private GameObject profileObject;

    [Space(10)] [SerializeField] [Tooltip("프로필 이미지")]
    private Image profileImage;

    [SerializeField] [Tooltip("판다 스프라이트")] private Sprite pandaSprite;

    [SerializeField] [Tooltip("레드 판다 스프라이트")]
    private Sprite redPandaSprite;

    [Space(10)] [SerializeField] [Tooltip("유저의 급수와 닉네임을 표시할 텍스트")]
    private TextMeshProUGUI gradeAndNickname;

    public string nickname { get; private set; }
    public int grade { get; private set; }

    [Header("패널")]
    [SerializeField] [Tooltip("메인 씬에서 사용할 패널들")]
    private List<GameObject> mainScenePanels;

    /*
     * 패널 순서
     * 0: Main
     * 1: Gameplay
     * 2: Gibo
     * 3: Rank
     * 4: Setting
     * 5: Store(사용 여부 불확실)
     */
    [SerializeField] [Tooltip("메인 패널")] private GameObject mainPanel;

    [Header("팝업")]
    [SerializeField] [Tooltip("메인 씬에서 사용할 버튼 1개짜리 팝업")]
    private GameObject oneButtonPopupPanel;

    [SerializeField] [Tooltip("메인 씬에서 사용할 버튼 2개짜리 팝업")]
    private GameObject twoButtonPopupPanel;

    private OneButtonPanel oneButtonPopup;
    private TwoButtonPanel twoButtonPopup;

    public StatsManager statsManager { get; private set; }

    // 테스트용
    // public string username;
    // public string password;

    protected override void Awake()
    {
        base.Awake();
        oneButtonPopup = oneButtonPopupPanel.GetComponent<OneButtonPanel>();
        twoButtonPopup = twoButtonPopupPanel.GetComponent<TwoButtonPanel>();
    }

    private void Start()
    {
        statsManager = NetworkManager.Instance.statsManager;

        // 메인 씬 호출 시 유저 정보를 가져와서 띄워줌
        statsManager.GetUserInfo((response) =>
            {
                nickname = response.nickname;
                profileImage.sprite = (response.profileImage == 1) ? pandaSprite : redPandaSprite;
                grade = response.grade;

                gradeAndNickname.text = $"{grade}급 {nickname}";
                GameManager.Instance.SetUserInfo(response.username, response.nickname, response.grade,
                    response.profileImage);
            },
            (errorType) =>
            {
                switch (errorType)
                {
                    case StatsResponseType.CANNOT_FOUND_USER:
                        OpenOneButtonPopup("유저를 찾지 못했습니다.");
                        //TODO: 로그인 씬으로 되돌아가는 기능을 OpenOneButtonPopup에 추가
                        Debug.LogWarning("기본 정보 가져오기 실패 : 유저를 찾지 못했습니다.");
                        break;
                    case StatsResponseType.NOT_LOGGED_IN:
                        OpenOneButtonPopup("로그인 상태가 아닙니다.");
                        //TODO: 로그인 씬으로 되돌아가는 기능을 OpenOneButtonPopup에 추가
                        Debug.LogWarning("기본 정보 가져오기 실패 : 로그인 상태가 아닙니다.");
                        break;
                }
            });
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Main_Scene") return;

        // 메인 씬 호출 시 메인 패널을 띄워줌
        OpenMainPanel();
    }

    /// <summary>
    /// 지정한 패널만 보여주는 메소드
    /// </summary>
    /// <param name="panelToOpen">열고자 하는 패널의 GameObject</param>
    public void OpenPanel(GameObject panelToOpen)
    {
        foreach (GameObject panel in mainScenePanels)
        {
            // 만약 현재 패널이 열고자 하는 패널이라면 활성화하고, 아니라면 비활성화
            panel.SetActive(panel == panelToOpen);
        }

        int openedPanelIndex = mainScenePanels.IndexOf(panelToOpen);

        // 메인 패널이나 게임플레이 패널일때만 profileObject를 활성화
        bool shouldShowProfile = (openedPanelIndex == 0 || openedPanelIndex == 1);
        profileObject.SetActive(shouldShowProfile);
    }

    /// <summary>
    /// 메인 패널을 보여주는 메소드
    /// </summary>
    public void OpenMainPanel()
    {
        for (int i = 0; i < mainScenePanels.Count; i++)
        {
            mainScenePanels[i].SetActive(i == 0);
        }

        profileObject.SetActive(true);
    }

    /// <summary>
    /// 한 개짜리 버튼 팝업 열기
    /// </summary>
    /// <param name="message">팝업창에 띄울 메세지</param>
    /// <param name="onConfirm">버튼을 눌렀을 때 실행할 액션</param>
    public void OpenOneButtonPopup(string message, Action onConfirm = null)
    {
        oneButtonPopup.OpenWithSetMessageAndButtonEvent(message, onConfirm);
    }

    /// <summary>
    /// 두 개짜리 버튼 팝업 열기
    /// </summary>
    /// <param name="message">팝업창에 띄울 메세지</param>
    /// <param name="onConfirm">확인 버튼을 눌렀을 때 실행할 액션</param>
    /// <param name="onCancel">취소 버튼을 눌렀을 때 실행할 액션</param>
    public void OpenTwoButtonPopup(string message, Action onConfirm = null, Action onCancel = null)
    {
        twoButtonPopup.OpenWithSetMessageAndButtonEvent(message, onConfirm, onCancel);
    }
}