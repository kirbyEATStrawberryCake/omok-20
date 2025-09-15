using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class timer : MonoBehaviour
{
    [SerializeField] private Image timerImage;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float totalTime;

    private float currentTime;
    private bool _isPaused;

    private void Start()
    {
        ResetTimer();
        StartTimer();
    }

    private void Update()
    {
        if (_isPaused) return;

        currentTime -= Time.deltaTime;
        if (currentTime <= 0)
        {
            currentTime = 0;
            PauseTimer(); // Ÿ�ӿ��� �� Ÿ�̸� ����
        }

        timerImage.fillAmount = currentTime / totalTime;
        timerText.text = Mathf.CeilToInt(currentTime).ToString();
    }

    public void StartTimer()
    {
        _isPaused = false;
    }

    public void PauseTimer()
    {
        _isPaused = true;
    }

    public void ResetTimer()
    {
        currentTime = totalTime;
        timerImage.fillAmount = 1;
        timerText.text = Mathf.CeilToInt(currentTime).ToString();
    }

}
