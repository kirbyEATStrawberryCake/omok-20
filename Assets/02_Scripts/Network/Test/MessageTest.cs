using TMPro;
using UnityEngine;

public class MessageTest : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI message1;
    [SerializeField] private TextMeshProUGUI message2;
    [SerializeField] private TextMeshProUGUI identity;
    [SerializeField] private TextMeshProUGUI[] recordTexts;
    [SerializeField] private GameObject rankingObject;
    [SerializeField] private Transform rankingContentObject;

    public void SetMessage(int num, string message, Color color)
    {
        if (num == 1)
        {
            message1.text = message;
            message1.color = color;
        }
        else if (num == 2)
        {
            message2.text = message;
            message2.color = color;
        }
        else if (num == 3)
        {
            identity.text = message;
            identity.color = color;
        }
    }

    public void SetRecord(Record record, Color color)
    {
        recordTexts[0].text = "총 게임 수 : " + record.totalGames.ToString();
        recordTexts[1].text = "승리 : " + record.totalWins.ToString();
        recordTexts[2].text = "패배 : " + record.totalLoses.ToString();
        recordTexts[3].text = "승률 : " + record.winRate.ToString();
    }

    public void ClearAllMessage()
    {
        ClearMessage(1);
        ClearMessage(2);
        ClearMessage(3);
        ClearMessage(4);
        ClearRanking();
    }

    public void ClearMessage(int num)
    {
        if (num == 1)
        {
            message1.text = "";
            message1.color = Color.white;
        }
        else if (num == 2)
        {
            message2.text = "";
            message2.color = Color.white;
        }
        else if (num == 3)
        {
            identity.text = "";
            identity.color = Color.white;
        }
        else if (num == 4)
        {
            foreach (var text in recordTexts)
            {
                text.text = "";
                text.color = Color.white;
            }
        }
    }

    public void ClearRanking()
    {
        foreach (Transform child in rankingContentObject)
        {
            Destroy(child.gameObject);
        }
        rankingObject.SetActive(false);
    }
}