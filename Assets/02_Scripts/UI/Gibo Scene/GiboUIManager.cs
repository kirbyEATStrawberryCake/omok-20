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

    [Header("ȭ�� UI ����")]
    [SerializeField] private GameObject gibo;
    [SerializeField] private GameObject popUpGiboExit;
    [SerializeField] private Image leftProfile_Image;
    [SerializeField] private TextMeshProUGUI leftProfile_Grade;
    [SerializeField] private Image rightProfile_Image;
    [SerializeField] private TextMeshProUGUI rightProfile_Grade;

    [Header("������ �̹���")]
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
    /// currentIndex�� -1 : ó��
    /// currentIndex�� -2 : ������
    /// </summary>
    /// <param name="currentIndex"></param>
    private void UpdateButtonDisplay(int currentIndex) 
    {
        // �⺻��: ���� Ȱ��ȭ
        FirstButton.GetComponent<Button>().interactable = true;
        BeforeButton.GetComponent<Button>().interactable = true;
        AfterButton.GetComponent<Button>().interactable = true;
        LastButton.GetComponent<Button>().interactable = true;

        if (currentIndex == -1) // ���� ���� ����
        {
            // �� ó��: First, Before ��Ȱ��ȭ
            FirstButton.GetComponent<Button>().interactable = false;
            BeforeButton.GetComponent<Button>().interactable = false;
        }
        else if (currentIndex == -2) // ���� �� ��
        {
            // �� ��: After, Last ��Ȱ��ȭ
            AfterButton.GetComponent<Button>().interactable = false;
            LastButton.GetComponent<Button>().interactable = false;
        }
    }

    private void UpdateProfileImage(GameRecord curRecord) 
    {
        // Todo: �ڱ�� ����

        // ���� ���� ����
        rightProfile_Image.sprite = curRecord.otherProfileImage == 1 ? panda :
                             (curRecord.otherProfileImage == 2 ? red_panda : null);
        rightProfile_Grade.text = $"{curRecord.otherRank}�� {curRecord.otherPlayerNickname}";

    }


}
