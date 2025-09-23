using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rank;
    [SerializeField] private Image profileImage;
    [SerializeField] private TextMeshProUGUI gradeAndNickname;
    [SerializeField] private TextMeshProUGUI winAndLose;

    [SerializeField] private Sprite panda;
    [SerializeField] private Sprite redPanda;

    private Image backgroundImage;
    [SerializeField] private Color[] rankColor;

    private void Awake()
    {
        backgroundImage = GetComponent<Image>();
    }

    /// <summary>
    /// 각 랭킹 패널의 데이터를 설정
    /// </summary>
    /// <param name="rank">랭킹</param>
    /// <param name="profileImage">프로필 이미지(1: panda, 2: redPanda</param>
    /// <param name="grade">급수</param>
    /// <param name="nickname">닉네임</param>
    /// <param name="win">승리 수</param>
    /// <param name="lose">패배 수</param>
    public void SetData(int rank, int profileImage, int grade, string nickname, int win, int lose)
    {
        this.rank.text = rank.ToString();
        this.profileImage.sprite = profileImage == 1 ? panda : redPanda;
        gradeAndNickname.text = $"{grade}급 {nickname}";
        winAndLose.text = $"{win}승 {lose}패";

        if (rank == 1)
            backgroundImage.color = rankColor[0];
        else if (rank == 2)
            backgroundImage.color = rankColor[1];
        else if (rank == 3)
            backgroundImage.color = rankColor[2];
        else
            backgroundImage.color = rankColor[3];
    }
}