using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 단위로 시작 시간이 키 값으로 저장됨. ( 게임 씬에서 사용 )
/// 해당 키들만 기보 목록 리스트에 저장됨. ( 메인 씬(기보 목록 화면)에서 사용 )
/// 시작시간(키)을 통해 기보 내용을 불러옴 ( 기보 씬에서 사용 )
/// </summary>
/// 
[Serializable]
public class MoveData // 돌 움직임
{
    public int stoneColor; // 1 - "black" or 2 - "white"
    public int x;          // 좌표 X
    public int y;          // 좌표 Y
}

[Serializable]
public class GameRecord // 해당 게임의 정보
{
    public string startTime;           // 키 값게임 시작 시간 (yyyyMMdd_HHmmss)
    public string displayTime;         // 화면 표시용 (yyyy-MM-dd HH:mm:ss)
    public string otherPlayerNickname; // 흑 닉네임
    public int otherRank;              // 흑 급수
    public int otherProfileImage;      // 흑 프로필
    public List<MoveData> moves;       // 게임 내 수순 리스트
}

[Serializable]
public class GiboIndex // 기보 목록
{
    public List<string> startTimes = new List<string>();
}

public class GiboManager
{
    private const string INDEX_KEY = "Gibo_Index";

    private GameRecord curRecord;

    public void StartNewRecord()
    {
        curRecord = new GameRecord
        {
            startTime = DateTime.Now.ToString("yyyyMMdd_HHmmss"),
            displayTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            otherPlayerNickname = "플레이어 2",
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


    #region 저장 및 불러오기

    public void SaveCurrentRecord()
    {
        if (curRecord == null || curRecord.moves.Count == 0) return;

        SaveRecord(curRecord);
    }

    // 새로운 기록 저장
    private void SaveRecord(GameRecord record)
    {
        string key = record.startTime;
        string json = JsonUtility.ToJson(record, true);
        PlayerPrefs.SetString(key, json);

        // 인덱스 업데이트
        GiboIndex index = LoadIndex();
        if (!index.startTimes.Contains(record.startTime))
        {
            index.startTimes.Add(record.startTime);
            SaveIndex(index);
        }

        PlayerPrefs.Save();
        Debug.Log($"기보 저장 완료: {record.startTime}");
    }

    // 특정 기록 불러오기
    public GameRecord LoadRecord(string startTime)
    {
        if (PlayerPrefs.HasKey(startTime))
        {
            string json = PlayerPrefs.GetString(startTime);
            return JsonUtility.FromJson<GameRecord>(json);
        }

        return null;
    }

    // 전체 기록 불러오기
    public List<GameRecord> LoadAllRecords()
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

    #region 인덱스 관리

    public GiboIndex LoadIndex()
    {
        if (PlayerPrefs.HasKey(INDEX_KEY))
        {
            string json = PlayerPrefs.GetString(INDEX_KEY);
            return JsonUtility.FromJson<GiboIndex>(json);
        }

        return new GiboIndex();
    }

    public void SaveIndex(GiboIndex index)
    {
        string json = JsonUtility.ToJson(index, true);
        PlayerPrefs.SetString(INDEX_KEY, json);
    }

    #endregion

    #region 삭제

    // 특정 기록 삭제
    public void DeleteRecord(string startTime)
    {
        if (PlayerPrefs.HasKey(startTime))
        {
            PlayerPrefs.DeleteKey(startTime);

            GiboIndex index = LoadIndex();
            index.startTimes.Remove(startTime);
            SaveIndex(index);

            PlayerPrefs.Save();
            Debug.Log($"기보 삭제 완료: {startTime}");
        }
    }

    // 모든 기록 삭제
    public void ClearAllRecords()
    {
        GiboIndex index = LoadIndex();
        foreach (var startTime in index.startTimes)
        {
            if (PlayerPrefs.HasKey(startTime))
                PlayerPrefs.DeleteKey(startTime);
        }

        PlayerPrefs.DeleteKey(INDEX_KEY);
        PlayerPrefs.Save();
        Debug.Log("모든 기보 삭제 완료");
    }

    #endregion
}