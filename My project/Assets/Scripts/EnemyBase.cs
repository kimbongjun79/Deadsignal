using UnityEngine;
using UnityEngine.AI;

// 모든 Enemy의 공통 기능(추격, 체력, 사망)을 담당하는 베이스 클래스
[RequireComponent(typeof(NavMeshAgent))]
public abstract class EnemyBase : MonoBehaviour
{
    [Tooltip("추격 및 이동에 사용되는 NavMeshAgent")]
    [SerializeField] protected NavMeshAgent agent;

    [Tooltip("추격 대상 플레이어")]
    [SerializeField] protected Transform player;

    [Tooltip("체력 관리 컴포넌트")]
    [SerializeField] protected HealthSystemForDummies health;

    [Tooltip("애니메이션 재생용 Animator")]
    [SerializeField] protected Animator animator;

    [Tooltip("공격을 시작할 수 있는 최대 거리")]
    [SerializeField] protected float attackRange = 2f;

    [Tooltip("공격 쿨다운 시간 (초)")]
    [SerializeField] protected float attackCooldown = 2f;

    [Tooltip("다음 공격까지 남은 시간")]
    [SerializeField] protected float cooldownTimer;

    [Tooltip("현재 공격 중인지 여부 (이동/회전 고정)")]
    [SerializeField] protected bool isAttacking;

    [Tooltip("사망 애니메이션 재생 후 오브젝트 제거까지 대기 시간")]
    [SerializeField] protected float destroyDelay = 2f;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<HealthSystemForDummies>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    protected virtual void Update()
    {
        if (player == null) return;

        if (health != null && !health.IsAlive)
        {
            HandleDeath();
            return;
        }

        if (isAttacking) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            agent.isStopped = true;
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                StartAttack();
                animator.SetTrigger("Attack");
                cooldownTimer = attackCooldown;
            }
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            cooldownTimer = 0f;
        }
    }

    // 사망 처리 (공통)
    protected virtual void HandleDeath()
    {
        animator.SetTrigger("IsDead");
        agent.isStopped = true;

        // 다른 몬스터의 이동 경로를 막지 않도록 Avoidance 비활성화
        agent.radius = 0f;
        agent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.NoObstacleAvoidance;

        // 총알이 통과하도록 콜라이더 비활성화
        Collider col = GetComponentInChildren<Collider>();
        if (col != null)
            col.enabled = false;

        if (GameManager.Instance != null)
            GameManager.Instance.AddKill();

        enabled = false;
        Destroy(gameObject, destroyDelay);
    }


    // 각 몬스터별 공격 방식 구현 (근접/원거리에서 재정의)
    protected abstract void StartAttack();
}