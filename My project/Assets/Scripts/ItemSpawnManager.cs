using UnityEngine;
using UnityEngine.AI;

// 맵 내 무작위 NavMesh 위치에 주기적으로 아이템(회복/탄약)을 스폰
public class ItemSpawnManager : MonoBehaviour
{
    [Tooltip("스폰 대상 플레이어 Transform")]
    [SerializeField] private Transform player;

    [Tooltip("회복 아이템 프리펩")]
    [SerializeField] private GameObject healthItemPrefab;

    [Tooltip("탄약 아이템 프리펩")]
    [SerializeField] private GameObject ammoItemPrefab;

    [Tooltip("스폰 최소 거리 (플레이어 기준)")]
    [SerializeField] private float minSpawnDistance = 5f;

    [Tooltip("스폰 최대 거리 (플레이어 기준)")]
    [SerializeField] private float maxSpawnDistance = 20f;

    [Tooltip("아이템 스폰 주기 (초)")]
    [SerializeField] private float spawnInterval = 15f;

    [Tooltip("회복 아이템이 스폰될 확률 (0~1, 나머지는 탄약)")]
    [SerializeField] private float healthItemChance = 0.5f;

    [Tooltip("스폰 위치 주변 NavMesh 탐색 허용 반경")]
    [SerializeField] private float navMeshSampleRadius = 3f;

    [Tooltip("유효한 위치를 찾기 위한 최대 재시도 횟수")]
    [SerializeField] private int maxSpawnAttempts = 10;

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
        spawnTimer = spawnInterval;
    }

    private void Update()
    {
        if (player == null) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            Spawn();
            spawnTimer = spawnInterval;
        }
    }

    // 플레이어 기준 범위 내 NavMesh 유효 위치에 회복 또는 탄약 아이템을 스폰
    private void Spawn()
    {
        GameObject prefabToSpawn = Random.value < healthItemChance ? healthItemPrefab : ammoItemPrefab;
        if (prefabToSpawn == null) return;

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(minSpawnDistance, maxSpawnDistance);

            Vector3 offset = new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);
            Vector3 candidatePos = new Vector3(player.position.x + offset.x, player.position.y, player.position.z + offset.z);

            if (NavMesh.SamplePosition(candidatePos, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
            {
                Instantiate(prefabToSpawn, hit.position, Quaternion.identity);
                return;
            }
        }
    }
}