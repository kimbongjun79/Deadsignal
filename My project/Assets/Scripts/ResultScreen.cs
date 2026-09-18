using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

// 게임오버/클리어 화면 공용: 생존 시간과 처치 수를 표시
public class ResultScreen : MonoBehaviour
{
    [Tooltip("생존 시간 표시 텍스트")]
    [SerializeField] private TextMeshProUGUI survivedTimeText;

    [Tooltip("처치 수 표시 텍스트")]
    [SerializeField] private TextMeshProUGUI killCountText;

    [Tooltip("타이틀로 돌아가는 씬 이름")]
    [SerializeField] private string titleSceneName = "Title";

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        float survivedTime = PlayerPrefs.GetFloat("LastSurvivedTime", 0f);
        int killCount = PlayerPrefs.GetInt("LastKillCount", 0);

        int minutes = Mathf.FloorToInt(survivedTime / 60f);
        int seconds = Mathf.FloorToInt(survivedTime % 60f);

        survivedTimeText.text = $"Time Survived: {minutes:00}:{seconds:00}";
        killCountText.text = $"Zombies Killed: {killCount}";
    }

    // 버튼에서 호출: 타이틀로 복귀
    public void ReturnToTitle()
    {
        SceneManager.LoadScene(titleSceneName);
    }
}