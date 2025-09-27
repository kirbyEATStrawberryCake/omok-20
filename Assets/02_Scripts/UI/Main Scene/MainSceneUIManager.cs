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

    [Space(10)]
    [SerializeField] [Tooltip("프로필 이미지")] private Image profileImage;
    [SerializeField] [Tooltip("판다 스프라이트")] private Sprite pandaSprite;
    [SerializeField] [Tooltip("레드 판다 스프라이트")] private Sprite redPandaSprite;

    [Space(10)]
    [SerializeField] [Tooltip("유저의 급수와 닉네임을 표시할 텍스트")] private TextMeshProUGUI gradeAndNickname;

    [Header("패널")]
    [SerializeField] [Tooltip("메인 씬에서 사용할 패널들")] private List<GameObject> mainScenePanels;
    [SerializeField] [Tooltip("메인 패널")] private GameObject mainPanel;
    [SerializeField] [Tooltip("게임플레이 정보 패널")] private GameObject gameplayPanel;

    [Header("팝업")]
    [SerializeField] [Tooltip("메인 씬에서 사용할 버튼 1개짜리 팝업")] private GameObject oneButtonPopupPanel;
    [SerializeField] [Tooltip("메인 씬에서 사용할 버튼 2개짜리 팝업")] private GameObject twoButtonPopupPanel;

    private OneButtonPanel oneButtonPopup;
    private TwoButtonPanel twoButtonPopup;

    protected override void Awake()
    {
        base.Awake();
        oneButtonPopup = oneButtonPopupPanel.GetComponent<OneButtonPanel>();
        twoButtonPopup = twoButtonPopupPanel.GetComponent<TwoButtonPanel>();
    }

    private void Start()
    {
#if UNITY_EDITOR
        if (gameObject.scene.name == EditorSceneLoader.StartupSceneName)
        {
            Debug.Log($"<color=cyan>메인씬 테스트 시작</color>");
            OnSceneLoad(SceneManager.GetActiveScene(), LoadSceneMode.Single);
            return;
        }
#endif
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Main_Scene") return;

        // 메인 씬 호출 시 메인 패널을 띄워줌
        OpenPanel();
        SetUserProfile();
    }

    /// <summary>
    /// 지정한 패널만 보여주는 메소드
    /// </summary>
    /// <param name="panelToOpen">열고자 하는 패널의 GameObject</param>
    public void OpenPanel(GameObject panelToOpen = null)
    {
        panelToOpen = panelToOpen ?? mainPanel;
        foreach (GameObject panel in mainScenePanels)
        {
            // 만약 현재 패널이 열고자 하는 패널이라면 활성화하고, 아니라면 비활성화
            panel.SetActive(panel == panelToOpen);
        }

        // 메인 패널이나 게임플레이 패널일때만 profileObject를 활성화
        bool shouldShowProfile = (panelToOpen == mainPanel) || (panelToOpen == gameplayPanel);
        profileObject.SetActive(shouldShowProfile);
    }

    /// <summary>
    /// 한 개짜리 버튼 팝업 열기
    /// </summary>
    /// <param name="message">팝업창에 띄울 메세지</param>
    /// <param name="onConfirm">버튼을 눌렀을 때 실행할 액션</param>
    public void OpenOneButtonPopup(string message, Action onConfirm = null)
    {
        oneButtonPopup.Show<OneButtonPanel>(message).OnConfirm(onConfirm);
    }

    /// <summary>
    /// 두 개짜리 버튼 팝업 열기
    /// </summary>
    /// <param name="message">팝업창에 띄울 메세지</param>
    /// <param name="onConfirm">확인 버튼을 눌렀을 때 실행할 액션</param>
    /// <param name="onCancel">취소 버튼을 눌렀을 때 실행할 액션</param>
    public void OpenTwoButtonPopup(string message, Action onConfirm = null, Action onCancel = null)
    {
        twoButtonPopup.Show<TwoButtonPanel>(message).OnButtons(onConfirm, onCancel);
    }

    private void SetUserProfile()
    {
        UserInfo_Network userInfo = NetworkManager.Instance.userDataManager.UserInfo;
        profileImage.sprite = (userInfo.profileImage == 1) ? pandaSprite : redPandaSprite;
        gradeAndNickname.text = $"{userInfo.grade}급 {userInfo.nickname}";
    }
}