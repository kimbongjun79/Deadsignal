using UnityEngine;

// 플레이어 주변 특정 거리 범위 내에 Enemy를 주기적으로 스폰하는 매니저
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

    [Tooltip("스폰 주기 (초)")]
    [SerializeField] private float spawnInterval = 5f;

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

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            Spawn();
            spawnTimer = spawnInterval;
        }
    }

    // 플레이어 기준 minSpawnDistance ~ maxSpawnDistance 사이, 같은 y축 평지에 Enemy 스폰
    private void Spawn()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);

        Vector3 offset = new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);
        Vector3 spawnPos = new Vector3(player.position.x + offset.x, player.position.y, player.position.z + offset.z);

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}