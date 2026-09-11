using UnityEngine;
using UnityEngine.SceneManagement;

// 타이틀 화면에서 게임 시작 처리
public class TitleScreen : MonoBehaviour
{
    [Tooltip("게임 플레이 씬 이름")]
    [SerializeField] private string gameplaySceneName = "Gameplay";

    // 버튼에서 호출: 게임 시작
    public void StartGame()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }
}