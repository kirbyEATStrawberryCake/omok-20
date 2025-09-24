using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingCell : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rank;
    [SerializeField] private Image profileImage;
    [SerializeField] private TextMeshProUGUI gradeAndNickname;
    [SerializeField] private TextMeshProUGUI winAndLose;

    [SerializeField] private Sprite panda;
    [SerializeField] private Sprite redPanda;

    private Image backgroundImage;
    [SerializeField] private Color[] rankColor;

    public int Index { get; private set; }


    private void Awake()
    {
        backgroundImage = GetComponent<Image>();
    }

    /// <summary>
    /// 각 랭킹 패널의 데이터를 설정
    /// </summary>
    /// <param name="rankingUser">유저 정보</param>
    /// <param name="index">각 패널 인덱스</param>
    public void SetData(RankingUser rankingUser, int index)
    {
        this.rank.text = rankingUser.rank.ToString();
        this.profileImage.sprite = rankingUser.profileImage == 1 ? panda : redPanda;
        gradeAndNickname.text = $"{rankingUser.grade}급 {rankingUser.identity.nickname}";
        winAndLose.text = $"{rankingUser.record.totalWins}승 {rankingUser.record.totalLoses}패";
        this.Index = index;

        if (rankingUser.rank == 1)
            backgroundImage.color = rankColor[0];
        else if (rankingUser.rank == 2)
            backgroundImage.color = rankColor[1];
        else if (rankingUser.rank == 3)
            backgroundImage.color = rankColor[2];
        else
            backgroundImage.color = rankColor[3];
    }
}