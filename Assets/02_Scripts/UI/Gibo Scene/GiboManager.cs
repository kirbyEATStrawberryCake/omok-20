using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ���� ������ ���� �ð��� Ű ������ �����. ( ���� ������ ��� )
/// �ش� Ű�鸸 �⺸ ��� ����Ʈ�� �����. ( ���� ��(�⺸ ��� ȭ��)���� ��� )
/// ���۽ð�(Ű)�� ���� �⺸ ������ �ҷ��� ( �⺸ ������ ��� )
/// </summary>
/// 
[Serializable]
public class MoveData // �� ������
{
    public int stoneColor;    // 1 - "black" or 2 - "white"
    public int x;             // ��ǥ X
    public int y;             // ��ǥ Y
}

[Serializable]
public class GameRecord // �ش� ������ ����
{
    public string startTime;      // Ű ������ ���� �ð� (yyyyMMdd_HHmmss)
    public string displayTime;    // ȭ�� ǥ�ÿ� (yyyy-MM-dd HH:mm:ss)
    public string otherPlayerNickname;    // �� �г���
    public int otherRank;         // �� �޼�
    public int otherProfileImage; // �� ������
    public List<MoveData> moves;  // ���� �� ���� ����Ʈ
}

[Serializable]
public class GiboIndex // �⺸ ���
{
    public List<string> startTimes = new List<string>();
}

public class GiboManager : Singleton<GiboManager>
{
    private const string INDEX_KEY = "Gibo_Index";

    private GameRecord curRecord;

    public void StartNewRecord()
    {
        curRecord = new GameRecord
        {
            startTime = DateTime.Now.ToString("yyyyMMdd_HHmmss"),
            displayTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            otherPlayerNickname = "�÷��̾� 2",
            otherProfileImage = 2,
            otherRank = 0,
            moves = new List<MoveData>()
        };
    }

    public void SetGiboProfileData(string nickname, int profileImg, int rank)
    {
        curRecord.otherPlayerNickname = nickname;
        curRecord.otherProfileImage = profileImg;
        curRecord.otherRank = rank;
    }
    public void AddMove(MoveData move)
    {
        if (curRecord == null) return;

        curRecord.moves.Add(move);
    }


    #region ���� �� �ҷ�����
    public void SaveCurrentRecord()
    {
        if (curRecord == null) return;

        SaveRecord(curRecord);
    }

    // ���ο� ��� ����
    public static void SaveRecord(GameRecord record)
    {
        string key = record.startTime;
        string json = JsonUtility.ToJson(record, true);
        PlayerPrefs.SetString(key, json);

        // �ε��� ������Ʈ
        GiboIndex index = LoadIndex();
        if (!index.startTimes.Contains(record.startTime))
        {
            index.startTimes.Add(record.startTime);
            SaveIndex(index);
        }

        PlayerPrefs.Save();
        Debug.Log($"�⺸ ���� �Ϸ�: {record.startTime}");
    }

    // Ư�� ��� �ҷ�����
    public static GameRecord LoadRecord(string startTime)
    {
        if (PlayerPrefs.HasKey(startTime))
        {
            string json = PlayerPrefs.GetString(startTime);
            return JsonUtility.FromJson<GameRecord>(json);
        }
        return null;
    }

    // ��ü ��� �ҷ�����
    public static List<GameRecord> LoadAllRecords()
    {
        List<GameRecord> allRecords = new List<GameRecord>();
        GiboIndex index = LoadIndex();

        foreach (var startTime in index.startTimes)
        {
            GameRecord record = LoadRecord(startTime);
            if (record != null)
                allRecords.Add(record);
        }

        return allRecords;
    }

    #endregion

    #region �ε��� ����

    public static GiboIndex LoadIndex()
    {
        if (PlayerPrefs.HasKey(INDEX_KEY))
        {
            string json = PlayerPrefs.GetString(INDEX_KEY);
            return JsonUtility.FromJson<GiboIndex>(json);
        }
        return new GiboIndex();
    }

    public static void SaveIndex(GiboIndex index)
    {
        string json = JsonUtility.ToJson(index, true);
        PlayerPrefs.SetString(INDEX_KEY, json);
    }

    #endregion

    #region ����

    // Ư�� ��� ����
    public static void DeleteRecord(string startTime)
    {
        if (PlayerPrefs.HasKey(startTime))
        {
            PlayerPrefs.DeleteKey(startTime);

            GiboIndex index = LoadIndex();
            index.startTimes.Remove(startTime);
            SaveIndex(index);

            PlayerPrefs.Save();
            Debug.Log($"�⺸ ���� �Ϸ�: {startTime}");
        }
    }

    // ��� ��� ����
    public static void ClearAllRecords()
    {
        GiboIndex index = LoadIndex();
        foreach (var startTime in index.startTimes)
        {
            if (PlayerPrefs.HasKey(startTime))
                PlayerPrefs.DeleteKey(startTime);
        }
        PlayerPrefs.DeleteKey(INDEX_KEY);
        PlayerPrefs.Save();
        Debug.Log("��� �⺸ ���� �Ϸ�");
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        throw new NotImplementedException();
    }

    #endregion
}