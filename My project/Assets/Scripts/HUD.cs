using UnityEngine;
using UnityEngine.UI;

// 남은 시간과 처치 수를 화면에 표시하는 HUD
public class HUD : MonoBehaviour
{
    [Tooltip("남은 시간 표시 텍스트")]
    [SerializeField] private Text timeText;

    [Tooltip("처치 수 표시 텍스트")]
    [SerializeField] private Text killText;

    private void Update()
    {
        if (GameManager.Instance == null) return;

        float remaining = Mathf.Max(0f, GameManager.Instance.SurviveTime - GameManager.Instance.ElapsedTime);
        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);
        timeText.text = $"{minutes:00}:{seconds:00}";

        killText.text = $"Kills: {GameManager.Instance.KillCount}";
    }
}