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