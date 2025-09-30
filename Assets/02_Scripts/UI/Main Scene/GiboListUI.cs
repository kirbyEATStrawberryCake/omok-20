using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GiboListUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform contentParent;   // ScrollView의 Content
    [SerializeField] private GameObject listItemPrefab; // 버튼 프리팹

    private readonly List<GameObject> spawnedItems = new();
    private readonly GiboManager giboManager = new();

    private GameRecord currentRecord;

    private void OnEnable()
    {
        LoadGiboList();
    }

    private void LoadGiboList()
    {
        // 기존 리스트 제거
        foreach (var item in spawnedItems)
        {
            Destroy(item);
        }
        spawnedItems.Clear();

        // PlayerPrefs에서 기보 인덱스 로드
        GiboIndex giboIndex = giboManager.LoadIndex();
        if (giboIndex == null || giboIndex.startTimes.Count == 0)
        {
            Debug.Log("저장된 기보가 없습니다.");
            return;
        }

        // 기보 키(startTime) 순회하며 버튼 생성
        foreach (string startTime in giboIndex.startTimes)
        {
            GameRecord record = giboManager.LoadRecord(startTime);
            if (record == null) continue;

            GameObject item = Instantiate(listItemPrefab, contentParent);
            spawnedItems.Add(item);

            // 버튼의 텍스트 변경
            TMP_Text text = item.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = $"{record.displayTime}";
            }

            // 버튼 이벤트 등록
            Button button = item.GetComponent<Button>();
            if (button != null)
            {
                string selectedId = startTime; // 클로저 방지
                button.onClick.AddListener(() => OnClickGiboItem(selectedId));
            }
        }
    }

    private void OnClickGiboItem(string gameId)
    {
        Debug.Log($"선택된 기보 ID: {gameId}");
        SelectedGiboGameId.selectedGameId = gameId;

        // 기보 재생 씬으로 이동
        SceneController.LoadScene(SceneType.Gibo);
    }
}