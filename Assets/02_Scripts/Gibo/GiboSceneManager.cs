using UnityEngine;

[RequireComponent(typeof(GiboBoardManager))]
[RequireComponent(typeof(GiboUIManager))]
public class GiboSceneManager : MonoBehaviour
{
    [SerializeField] private GameObject popupObject;
    [SerializeField] private TwoButtonPanel twoButtonPopup;

    private GiboBoardManager giboBoardManager;
    private GiboUIManager giboUIManager;
    private readonly GiboManager giboManager = new();

    private GameRecord currentRecord;
    private int currentMoveIndex = -1;

    private void Awake()
    {
        giboBoardManager = GetComponent<GiboBoardManager>();
        giboUIManager = GetComponent<GiboUIManager>();
    }

    private void Start()
    {
        LoadSelectedRecord();
        giboBoardManager.SetRecord(currentRecord);
        giboUIManager.UpdateButtonDisplay(currentMoveIndex);
        giboUIManager.UpdateProfileImage(currentRecord);
    }

    private void LoadSelectedRecord()
    {
        string gameId = SelectedGiboGameId.selectedGameId;

        if (string.IsNullOrEmpty(gameId))
        {
            Debug.LogError($"{gameId}");
            return;
        }

        currentRecord = giboManager.LoadRecord(gameId);
        if (currentRecord == null)
        {
            Debug.LogError($"기보를 찾을 수 없습니다: {gameId}");
            return;
        }

        Debug.Log(currentRecord.otherPlayerNickname + " " + currentRecord.otherProfileImage + " " + currentRecord.otherRank);
    }

    public void OnClickFirst()
    {
        if (currentRecord == null) return;

        currentMoveIndex = -1;
        giboBoardManager.ShowFirst();
        giboUIManager.UpdateButtonDisplay(currentMoveIndex);
    }

    public void OnClickPrevious()
    {
        if (currentRecord == null) return;

        if (currentMoveIndex == -2)
            currentMoveIndex = currentRecord.moves.Count - 1;

        currentMoveIndex--;

        giboBoardManager.ShowPrevious(currentMoveIndex);
        giboUIManager.UpdateButtonDisplay(currentMoveIndex);
    }

    public void OnClickNext()
    {
        if (currentRecord == null || currentMoveIndex >= currentRecord.moves.Count - 1) return;

        currentMoveIndex++;

        giboBoardManager.ShowNext(currentMoveIndex);

        if (currentMoveIndex == currentRecord.moves.Count - 1)
            currentMoveIndex = -2;

        giboUIManager.UpdateButtonDisplay(currentMoveIndex);
        SoundManager.PlaySFX();
    }

    public void OnClickLast()
    {
        if (currentRecord == null) return;

        currentMoveIndex = -2;
        giboBoardManager.ShowLast();
        giboUIManager.UpdateButtonDisplay(currentMoveIndex);
        SoundManager.PlaySFX();
    }

    public void OnClickExit()
    {
        popupObject.SetActive(true);

        twoButtonPopup.Show<TwoButtonPanel>("기보 보기를 종료하시겠습니까?")
            .WithButtonText("종료", "취소")
            .OnButtons(() =>
            {
                popupObject.SetActive(false);
                SceneController.LoadScene(SceneType.Main);
            }, () => { popupObject.SetActive(false); });
    }
}