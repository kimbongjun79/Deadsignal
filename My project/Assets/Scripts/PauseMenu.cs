using UnityEngine;
using UnityEngine.SceneManagement;

// ESC 키로 게임 일시정지/재개를 처리하는 매니저
public class PauseMenu : MonoBehaviour
{
    [Tooltip("일시정지 메뉴 UI 루트 오브젝트")]
    [SerializeField] private GameObject pauseMenuUI;

    [Tooltip("타이틀로 돌아가는 씬 이름")]
    [SerializeField] private string titleSceneName = "Title";

    public static bool IsPaused { get; private set; }

    private void Awake()
    {
        IsPaused = false;
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused) Resume();
            else Pause();
        }
    }

    // 게임 일시정지
    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // 게임 재개 (버튼에서도 호출)
    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // 버튼에서 호출: 타이틀로 나가기
    public void QuitToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(titleSceneName);
    }
}