using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.Multiplayer.Playmode;
using UnityEngine.Events;

public class GiboBoardManager : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite blackStoneSprite; // �浹 ��������Ʈ

    [SerializeField] private Sprite whiteStoneSprite; // �鵹 ��������Ʈ

    [Header("Board Settings")]
    [SerializeField] private float cellSize = 0.494f; // �� ĭ�� ũ��

    public int boardSize { get; private set; } = 15; // ������ ũ�� (15x15)
    [SerializeField] private Vector2 boardOffset = Vector2.zero; // ���� ������

    [Header("Maker Object")]
    [SerializeField] private GameObject lastMoveMarker; // ������ �� ��Ŀ

    [Header("Marker Settings")]
    [SerializeField] private GameObject stonePrefab_White;

    [SerializeField] private GameObject stonePrefab_Black;
    [SerializeField] private Transform stoneParent; // ���� ������ �θ� ������Ʈ
    [Space(10)][SerializeField] private GameObject forbiddenMarkerPrefab; // ���� ��ũ ������
    [SerializeField] private Transform forbiddenMarkerParent; // ���� ��ũ�� ������ �θ� ������Ʈ

    protected StoneType[,] board; // ������ �迭 (���� ����)
    private GameObject[,] stoneObjects; // �� ������Ʈ �迭

    private List<GameObject> forbiddenMarkers; // ���� ��ġ ��Ŀ��

    // �⺸��
    private GameRecord currentRecord;
    private int currentMoveIndex = -1;

    // �̺�Ʈ
    public event UnityAction<int> OnButtonChanged;
    public event UnityAction<GameRecord> OnProfileImage;


    #region ����Ƽ �̺�Ʈ

    private void Awake()
    {
        InitializeBoard();
        CreateTestRecord();
        //LoadSelectedRecord();
    }

    #endregion

    #region �ʱ�ȭ

    /// <summary>
    /// ������ �ʱ�ȭ
    /// </summary>
    private void InitializeBoard()
    {
        // �迭 �ʱ�ȭ
        board = new StoneType[boardSize, boardSize];
        stoneObjects = new GameObject[boardSize, boardSize];
        forbiddenMarkers = new List<GameObject>();

        // ���� ������Ʈ�� ����
        ClearBoard();
    }

    /// <summary>
    /// ������ ����
    /// </summary>
    public void ClearBoard()
    {
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                board[x, y] = StoneType.None;

                if (stoneObjects[x, y] != null)
                {
                    Destroy(stoneObjects[x, y]);
                    stoneObjects[x, y] = null;
                }
            }
        }

        HideAllMarkers();
    }

    /// <summary>
    /// ��Ŀ�� ����
    /// </summary>
    private void HideAllMarkers(GameResult result = GameResult.None)
    {
        lastMoveMarker?.SetActive(false); // ������ �� ��Ŀ �����
        HideForbiddenMarkers(); // ���� ��ġ ��Ŀ�� �����
    }

    /// <summary>
    /// ���� ��ġ ��Ŀ�� �����
    /// </summary>
    private void HideForbiddenMarkers()
    {
        foreach (GameObject marker in forbiddenMarkers)
        {
            if (marker != null)
            {
                Destroy(marker);
            }
        }

        forbiddenMarkers.Clear();
    }

    #endregion

    #region ���콺 �Է� �� ���� ���


    /// <summary>
    /// ���� ��ǥ�� ���� ��ǥ�� ��ȯ
    /// </summary>
    private Vector3 BoardToWorldPosition(int x, int y)
    {
        float worldX = (x - (boardSize - 1) / 2.0f) * cellSize + boardOffset.x;
        float worldY = ((boardSize - 1) / 2.0f - y) * cellSize + boardOffset.y; // Y�� ����
        return transform.position + new Vector3(worldX, worldY, 0);
    }

    #endregion

    #region ����

    /// <summary>
    /// �ð��� �� ��������Ʈ ���� �� ������ �� ��Ŀ ������Ʈ
    /// </summary>
    public void PlaceStoneVisual(int x, int y, StoneType stoneType)
    {
        // �ð��� �� ��������Ʈ ����
        Vector3 worldPos = BoardToWorldPosition(x, y);
        var stoneObj = Instantiate(stoneType == StoneType.Black ? stonePrefab_Black : stonePrefab_White,
            worldPos, Quaternion.identity, stoneParent);
        stoneObj.name = $"Stone_{x}_{y}";
        stoneObjects[x, y] = stoneObj;

        // ������ �� ��Ŀ ������Ʈ
        UpdateLastMoveMarker(x, y);
    }

    #endregion

    #region ������ �� ��Ŀ

    /// <summary>
    /// ������ �� ��Ŀ ������Ʈ
    /// </summary>
    private void UpdateLastMoveMarker(int x, int y)
    {
        lastMoveMarker.transform.position = BoardToWorldPosition(x, y);
        lastMoveMarker.SetActive(true);
    }

    #endregion

    #region �⺸�� - �ش� ��ǥ �� ����, ������ ��Ŀ �����
    /// <summary>
    /// �ش� ��ǥ�� �� ����
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void RemoveStone(int x, int y)
    {
        // ���� ���� �ʱ�ȭ
        board[x, y] = StoneType.None;

        // �ð��� �� ����
        if (stoneObjects[x, y] != null)
        {
            Destroy(stoneObjects[x, y]);
            stoneObjects[x, y] = null;
        }

        // ������ �� ��Ŀ ������Ʈ
        lastMoveMarker.SetActive(false);
    }

    /// <summary>
    /// �⺸�� - ������ �� ��Ŀ ����� 
    /// </summary>
    public void HideLastMoveMarker()
    {
        if (lastMoveMarker != null)
            lastMoveMarker.SetActive(false);
    }
    #endregion

    private void LoadSelectedRecord()
    {
        string gameId = SelectedGiboGameId.selectedGameId;

        if (string.IsNullOrEmpty(gameId))
        {
            Debug.LogError($"{gameId}");
            return;
        }

        currentRecord = GiboManager.LoadRecord(gameId);
        if (currentRecord == null)
        {
            Debug.LogError($"�⺸�� ã�� �� �����ϴ�: {gameId}");
            return;
        }

        // �ʱ�ȭ: ���� ����
        ClearBoard();
        currentMoveIndex = -1; // ���� ���� ������ ���� ����
        OnProfileImage?.Invoke(currentRecord);
    }

    private void CreateTestRecord()
    {
        // �׽�Ʈ�� ID ����
        SelectedGiboGameId.selectedGameId = "20250919_120000";

        // �׽�Ʈ�� GameRecord ����
        currentRecord = new GameRecord
        {
            startTime = "20250919_120000", // Ű ����
            displayTime = "2025-09-19 12:00", // ȭ�� ǥ�ÿ�
            otherPlayerNickname = "ȫ�浿",
            otherRank = 3,
            otherProfileImage = 1,
            moves = new System.Collections.Generic.List<MoveData>()
        };

        currentRecord.moves.Add(new MoveData { turn = 1, stoneColor = 1, x = 7, y = 7 }); // �� �߾�
        currentRecord.moves.Add(new MoveData { turn = 2, stoneColor = 2, x = 7, y = 8 }); // �� �Ʒ�
        currentRecord.moves.Add(new MoveData { turn = 3, stoneColor = 1, x = 8, y = 8 }); // �� �밢
        currentRecord.moves.Add(new MoveData { turn = 4, stoneColor = 2, x = 6, y = 7 }); // �� ����
        currentRecord.moves.Add(new MoveData { turn = 5, stoneColor = 1, x = 8, y = 7 }); // �� ������
    }

    #region ��ư �̺�Ʈ (ó�� / ���� / ���� / ��)

    public void OnClickFirst()
    {
        ClearBoard();
        currentMoveIndex = -1;
        OnButtonChanged?.Invoke(currentMoveIndex);
    }

    public void OnClickPrevious()
    {
        if (currentMoveIndex < 0) return;

        // ������ �� ����
        MoveData lastMove = currentRecord.moves[currentMoveIndex];
        RemoveStone(lastMove.x, lastMove.y);
        currentMoveIndex--;
        OnButtonChanged?.Invoke(currentMoveIndex);
    }

    public void OnClickNext()
    {
        if (currentRecord == null || currentMoveIndex >= currentRecord.moves.Count - 1) return;

        currentMoveIndex++;
        MoveData move = currentRecord.moves[currentMoveIndex];
        PlaceStoneVisual(move.x, move.y, move.stoneColor == 1 ? StoneType.Black : StoneType.White);
        if(currentMoveIndex == currentRecord.moves.Count -1)
            OnButtonChanged?.Invoke(-2);
        else
        {
            OnButtonChanged?.Invoke(currentMoveIndex);
        }
    }

    public void OnClickLast()
    {
        if (currentRecord == null) return;

        // ���� �ʱ�ȭ �� ��� �� ��ġ
        ClearBoard();
        for (int i = 0; i < currentRecord.moves.Count; i++)
        {
            MoveData move = currentRecord.moves[i];
            PlaceStoneVisual(move.x, move.y, move.stoneColor == 1 ? StoneType.Black : StoneType.White);
        }
        currentMoveIndex = currentRecord.moves.Count - 1;
        OnButtonChanged?.Invoke(-2); // -2 ���� ���� ��� �������̶�� �ν�
    }

    #endregion
}