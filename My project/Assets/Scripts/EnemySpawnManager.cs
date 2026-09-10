using UnityEngine;

// 플레이어 주변 특정 거리 범위 내에 Enemy를 주기적으로 스폰하는 매니저
// 경과 시간에 따라 스폰 빈도(개수)와 체력 배율이 지수 곡선으로 증가
public class EnemySpawnManager : MonoBehaviour
{
    [Tooltip("스폰 대상 플레이어 Transform")]
    [SerializeField] private Transform player;

    [Tooltip("스폰할 Enemy 프리펩")]
    [SerializeField] private GameObject enemyPrefab;

    [Tooltip("스폰 최소 거리")]
    [SerializeField] private float minSpawnDistance = 15f;

    [Tooltip("스폰 최대 거리")]
    [SerializeField] private float maxSpawnDistance = 18f;

    [Header("난이도 증가 - 공통 설정")]
    [Tooltip("난이도가 최대치에 도달하는 시간 (초). 예: 600 = 10분")]
    [SerializeField] private float timeToMax = 600f;

    [Tooltip("증가 곡선 강도. 클수록 후반에 급격히 증가 (예: 3~4 권장)")]
    [SerializeField] private float curveExponent = 3f;

    [Header("스폰 주기 (초당 스폰 빈도로 사용)")]
    [Tooltip("게임 시작 시 스폰 간격")]
    [SerializeField] private float startSpawnInterval = 5f;

    [Tooltip("최대 난이도 도달 시 스폰 간격 (값이 작을수록 자주 스폰)")]
    [SerializeField] private float minSpawnInterval = 1f;

    [Header("몬스터 체력 배율")]
    [Tooltip("게임 시작 시 체력 배율")]
    [SerializeField] private float startHealthMultiplier = 1f;

    [Tooltip("최대 난이도 도달 시 체력 배율")]
    [SerializeField] private float maxHealthMultiplier = 5f;

    [Tooltip("현재 경과 시간")]
    [SerializeField] private float elapsedTime;

    [Tooltip("다음 스폰까지 남은 시간")]
    [SerializeField] private float spawnTimer;

    private void Awake()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (player == null || enemyPrefab == null) return;

        elapsedTime += Time.deltaTime;

        float currentInterval = GetCurrentSpawnInterval();

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            Spawn();
            spawnTimer = currentInterval;
        }
    }

    // 경과 시간에 따른 0~1 진행도를 지수 곡선으로 변환 (초반 완만, 후반 급증)
    private float GetProgressFactor()
    {
        float normalizedT = Mathf.Clamp01(elapsedTime / timeToMax);
        return Mathf.Pow(normalizedT, curveExponent);
    }

    // 현재 스폰 간격 계산 (시작값 → 최소값으로 감소)
    private float GetCurrentSpawnInterval()
    {
        float factor = GetProgressFactor();
        return Mathf.Lerp(startSpawnInterval, minSpawnInterval, factor);
    }

    // 현재 체력 배율 계산 (시작값 → 최대값으로 증가)
    private float GetCurrentHealthMultiplier()
    {
        float factor = GetProgressFactor();
        return Mathf.Lerp(startHealthMultiplier, maxHealthMultiplier, factor);
    }

    // 플레이어 기준 minSpawnDistance ~ maxSpawnDistance 사이, 같은 y축 평지에 Enemy 스폰
    private void Spawn()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);

        Vector3 offset = new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);
        Vector3 spawnPos = new Vector3(player.position.x + offset.x, player.position.y, player.position.z + offset.z);

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        HealthSystemForDummies health = enemy.GetComponent<HealthSystemForDummies>();
        if (health != null)
        {
            float multiplier = GetCurrentHealthMultiplier();
            health.AddToMaximumHealth(health.MaximumHealth * (multiplier - 1f));
            health.ReviveWithMaximumHealth();
        }
    }
}