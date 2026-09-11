using UnityEngine;
using UnityEngine.SceneManagement;

// 게임 진행 상태(시간, 킬 카운트)를 관리하고 클리어/오버 조건을 판정하는 매니저
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Tooltip("클리어까지 버텨야 하는 시간 (초). 기본 10분")]
    [SerializeField] private float surviveTime = 600f;

    [Tooltip("현재 경과 시간")]
    [SerializeField] private float elapsedTime;

    [Tooltip("처치한 좀비 수")]
    [SerializeField] private int killCount;

    [Tooltip("게임이 이미 종료되었는지 여부 (중복 처리 방지)")]
    [SerializeField] private bool isGameOver;

    public float ElapsedTime => elapsedTime;
    public int KillCount => killCount;
    public float SurviveTime => surviveTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (isGameOver) return;

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= surviveTime)
            TriggerClear();
    }

    // 좀비 처치 시 EnemyBase에서 호출
    public void AddKill()
    {
        if (isGameOver) return;
        killCount++;
    }

    // 플레이어 사망 시 PlayerController에서 호출
    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        PlayerPrefs.SetFloat("LastSurvivedTime", elapsedTime);
        PlayerPrefs.SetInt("LastKillCount", killCount);

        SceneManager.LoadScene("GameOver");
    }

    // 10분 생존 달성 시 호출
    private void TriggerClear()
    {
        isGameOver = true;

        PlayerPrefs.SetFloat("LastSurvivedTime", elapsedTime);
        PlayerPrefs.SetInt("LastKillCount", killCount);

        SceneManager.LoadScene("Clear");
    }
}