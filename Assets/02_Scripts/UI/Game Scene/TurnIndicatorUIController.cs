using UnityEngine;

public class TurnIndicatorUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject blackStonePlayerTurnImage; // 흑돌 플레이어 차례 표시 GameObject
    [SerializeField] private GameObject whiteStonePlayerTurnImage; // 백돌 플레이어 차례 표시 GameObject
    [SerializeField] private GameObject currentPlayerTurnCircleLeft; // 현재 턴 플레이어 표시용 GameObject / Left
    [SerializeField] private GameObject currentPlayerTurnCircleRight; // 현재 턴 플레이어 표시용 GameObject / Right
    
    private void Start()
    {
        InitPlayerTurnDisplay();
    }

    /// <summary>
    /// 플레이어 차레 표시 초기화
    /// </summary>
    private void InitPlayerTurnDisplay()
    {
        blackStonePlayerTurnImage?.SetActive(true);
        whiteStonePlayerTurnImage?.SetActive(false);
    }

    /// <summary>
    /// 플레이어 차례 표시 업데이트
    /// </summary>
    public void UpdatePlayerTurnDisplay(StoneType stoneType, PlayerType playerType)
    {
        // 흑돌/백돌 차례 표시 이미지 업데이트
        blackStonePlayerTurnImage?.SetActive(stoneType == StoneType.Black);
        whiteStonePlayerTurnImage?.SetActive(stoneType == StoneType.White);

        // 현재 턴 사용자 표시 업데이트
        currentPlayerTurnCircleLeft.SetActive(playerType == PlayerType.Player1 || playerType == PlayerType.Me);
        currentPlayerTurnCircleRight.SetActive(playerType == PlayerType.Player2 || playerType == PlayerType.Opponent ||
                                               playerType == PlayerType.AI);
    }
}