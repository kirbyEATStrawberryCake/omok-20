using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GiboListUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform contentParent;   // ScrollView�� Content
    [SerializeField] private GameObject listItemPrefab; // ��ư ������

    private readonly List<GameObject> spawnedItems = new();

    private GameRecord currentRecord;

    private void OnEnable()
    {
        LoadGiboList();
    }


    private void LoadGiboList()
    {
        // ���� ����Ʈ ����
        foreach (var item in spawnedItems)
        {
            Destroy(item);
        }
        spawnedItems.Clear();

        // PlayerPrefs���� �⺸ �ε��� �ε�
        GiboIndex giboIndex = GiboManager.LoadIndex();
        if (giboIndex == null || giboIndex.startTimes.Count == 0)
        {
            Debug.Log("����� �⺸�� �����ϴ�.");
            return;
        }

        // �⺸ Ű(startTime) ��ȸ�ϸ� ��ư ����
        foreach (string startTime in giboIndex.startTimes)
        {
            GameRecord record = GiboManager.LoadRecord(startTime);
            if (record == null) continue;

            GameObject item = Instantiate(listItemPrefab, contentParent);
            spawnedItems.Add(item);

            // ��ư�� �ؽ�Ʈ ����
            TMP_Text text = item.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = $"{record.displayTime}";
            }

            // ��ư �̺�Ʈ ���
            Button button = item.GetComponent<Button>();
            if (button != null)
            {
                string selectedId = startTime; // Ŭ���� ����
                button.onClick.AddListener(() => OnClickGiboItem(selectedId));
            }
        }
    }

    private void OnClickGiboItem(string gameId)
    {
        Debug.Log($"���õ� �⺸ ID: {gameId}");
        SelectedGiboGameId.selectedGameId = gameId;

        // �⺸ ��� ������ �̵� (�� �̸� ���� �ʿ�)
        SceneController.LoadScene(SceneType.Gibo);
    }
}
