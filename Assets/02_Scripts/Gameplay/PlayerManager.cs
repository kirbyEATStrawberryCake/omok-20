using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Settings")]
    public bool blackPlayerStarts = true;      // �浹�� ���� �����ϴ��� ����

    private StoneType currentPlayer;            // ���� �÷��̾�
    private int blackWins = 0;                  // �浹 �¼�
    private int whiteWins = 0;                  // �鵹 �¼�
    private int draws = 0;                      // ���º� Ƚ��

    // �÷��̾� ���� �� �߻��ϴ� �̺�Ʈ
    public System.Action<StoneType> OnPlayerChanged;

    void Start()
    {
        InitializePlayers();
    }

    /// <summary>
    /// �÷��̾� �ʱ�ȭ
    /// </summary>
    public void InitializePlayers()
    {
        // ù ��° �÷��̾� ���� (�Ϲ������� �浹����)
        currentPlayer = blackPlayerStarts ? StoneType.Black : StoneType.White;

        Debug.Log($"���� ����! {GetCurrentPlayerName()}���� �����մϴ�.");

        // �̺�Ʈ �߻�
        OnPlayerChanged?.Invoke(currentPlayer);
    }

    /// <summary>
    /// �÷��̾� �� ��ü
    /// </summary>
    public void SwitchPlayer()
    {
        currentPlayer = (currentPlayer == StoneType.Black) ? StoneType.White : StoneType.Black;

        Debug.Log($"�� ��ü: ���� �÷��̾�� {GetCurrentPlayerName()}");

        // �̺�Ʈ �߻�
        OnPlayerChanged?.Invoke(currentPlayer);
    }

    /// <summary>
    /// ���� �÷��̾� ��ȯ
    /// </summary>
    /// <returns>���� �÷��̾��� �� Ÿ��</returns>
    public StoneType GetCurrentPlayer()
    {
        return currentPlayer;
    }

    /// <summary>
    /// ���� �÷��̾� �̸� ��ȯ
    /// </summary>
    /// <returns>���� �÷��̾� �̸� (�浹/�鵹)</returns>
    public string GetCurrentPlayerName()
    {
        return currentPlayer == StoneType.Black ? "�浹" : "�鵹";
    }

    /// <summary>
    /// ���� �÷��̾� Ÿ�� ��ȯ
    /// </summary>
    /// <returns>������ �� Ÿ��</returns>
    public StoneType GetOpponentPlayer()
    {
        return (currentPlayer == StoneType.Black) ? StoneType.White : StoneType.Black;
    }

    /// <summary>
    /// ���� ��� ���
    /// </summary>
    /// <param name="winner">�¸��� �÷��̾� (���ºν� StoneType.None)</param>
    public void RecordGameResult(StoneType winner)
    {
        switch (winner)
        {
            case StoneType.Black:
                blackWins++;
                Debug.Log($"�浹 �¸�! (�浹: {blackWins}��, �鵹: {whiteWins}��, ���º�: {draws}ȸ)");
                break;
            case StoneType.White:
                whiteWins++;
                Debug.Log($"�鵹 �¸�! (�浹: {blackWins}��, �鵹: {whiteWins}��, ���º�: {draws}ȸ)");
                break;
            case StoneType.None:
                draws++;
                Debug.Log($"���º�! (�浹: {blackWins}��, �鵹: {whiteWins}��, ���º�: {draws}ȸ)");
                break;
        }
    }

    /// <summary>
    /// ���� ��� ��ȯ
    /// </summary>
    /// <returns>�� �÷��̾��� �¼��� ���º� Ƚ��</returns>
    public (int blackWins, int whiteWins, int draws) GetGameRecord()
    {
        return (blackWins, whiteWins, draws);
    }

    /// <summary>
    /// ���� ��� �ʱ�ȭ
    /// </summary>
    public void ResetGameRecord()
    {
        blackWins = 0;
        whiteWins = 0;
        draws = 0;
        Debug.Log("���� ����� �ʱ�ȭ�Ǿ����ϴ�.");
    }

    /// <summary>
    /// ù ��° �÷��̾� ����
    /// </summary>
    /// <param name="stoneType">������ �÷��̾��� �� Ÿ��</param>
    public void SetFirstPlayer(StoneType stoneType)
    {
        if (stoneType == StoneType.Black || stoneType == StoneType.White)
        {
            currentPlayer = stoneType;
            OnPlayerChanged?.Invoke(currentPlayer);
        }
    }
}
