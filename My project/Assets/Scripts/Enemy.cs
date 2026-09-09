using UnityEngine;
using UnityEngine.AI;

// NavMesh를 이용해 플레이어를 추격하고 사거리 내에서 발사체를 발사하는 컴포넌트
[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    [Tooltip("추격 및 이동에 사용되는 NavMeshAgent 컴포넌트")]
    [SerializeField] private NavMeshAgent agent;

    [Tooltip("추격 대상 플레이어 Transform")]
    [SerializeField] private Transform player;

    [Tooltip("체력 관리 컴포넌트")]
    [SerializeField] private Health health;

    [Header("발사 설정")]
    [Tooltip("발사체를 발사할 수 있는 최대 거리")]
    [SerializeField] private float fireRange = 5f;

    [Tooltip("발사 쿨다운 시간 (초)")]
    [SerializeField] private float fireCooldown = 2f;

    [Tooltip("발사할 총알 프리펩")]
    [SerializeField] private GameObject bulletPrefab;

    [Tooltip("총알이 발사되는 위치")]
    [SerializeField] private Transform firePoint;

    [Tooltip("발사체 속도")]
    [SerializeField] private float bulletSpeed = 20f;

    [Tooltip("다음 발사까지 남은 시간")]
    [SerializeField] private float cooldownTimer;

    [Header("기즈모")]
    [Tooltip("발사 최대 범위 표시 여부")]
    [SerializeField] private bool showFireRangeGizmo = true;

    [Tooltip("전방 방향 화살표 표시 여부")]
    [SerializeField] private bool showForwardArrowGizmo = true;

    [Tooltip("전방 화살표 길이")]
    [SerializeField] private float arrowLength = 2f;

    private void Awake()
    {
        health = GetComponent<Health>();
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= fireRange)
        {
            // 사거리 내: 이동 정지 후 발사
            agent.isStopped = true;
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                Fire();
                cooldownTimer = fireCooldown;
            }
        }
        else
        {
            // 사거리 밖: 플레이어 추격
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }

        if (health != null && health.HP <= 0)
            gameObject.SetActive(false);
    }

    // firePoint 위치에서 bulletPrefab을 생성하고 전방으로 발사
    private void Fire()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
            bulletRb.linearVelocity = firePoint.forward * bulletSpeed;
    }

    private void OnDrawGizmos()
    {
        Vector3 origin = transform.position;

        // 발사 최대 범위 표시 (구체)
        if (showFireRangeGizmo)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(origin, fireRange);
        }

        // 전방 방향 화살표 표시
        if (showForwardArrowGizmo)
        {
            Gizmos.color = Color.blue;
            DrawArrow(origin, transform.forward * arrowLength);
        }
    }

    // 화살표 형태의 기즈모를 그리는 헬퍼 함수
    private void DrawArrow(Vector3 origin, Vector3 direction)
    {
        Vector3 end = origin + direction;
        Gizmos.DrawLine(origin, end);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 150f, 0f) * Vector3.forward * 0.3f;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, -150f, 0f) * Vector3.forward * 0.3f;

        Gizmos.DrawLine(end, end + right);
        Gizmos.DrawLine(end, end + left);
    }
}