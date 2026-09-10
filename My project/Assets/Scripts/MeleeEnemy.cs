using UnityEngine;

// 근접 공격 몬스터: 전방 히트박스 활성화 방식
public class MeleeEnemy : EnemyBase
{
    [Header("공격 타이밍")]
    [Tooltip("공격 시작 후 히트박스가 켜지기까지 대기 시간")]
    [SerializeField] private float attackDelay = 0.2f;

    [Tooltip("공격 판정 콜라이더가 활성화되는 시간 (초)")]
    [SerializeField] private float hitboxActiveDuration = 0.3f;

    [Tooltip("히트박스 비활성화 이후 다음 행동까지 대기 시간 (후딜레이)")]
    [SerializeField] private float postAttackDelay = 2f;

    [Tooltip("전방에 미리 배치해둔 공격 판정 오브젝트 (비활성 상태로 시작)")]
    [SerializeField] private GameObject attackHitbox;

    private AttackHitbox attackHitboxScript;

    protected override void Awake()
    {
        base.Awake();

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
            attackHitboxScript = attackHitbox.GetComponent<AttackHitbox>();
        }
    }

    protected override void StartAttack()
    {
        StartCoroutine(AttackRoutine());
    }

    private System.Collections.IEnumerator AttackRoutine()
    {
        isAttacking = true;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();

        yield return new WaitForSeconds(attackDelay);

        if (attackHitboxScript != null)
            attackHitboxScript.ResetHit();

        if (attackHitbox != null)
            attackHitbox.SetActive(true);

        yield return new WaitForSeconds(hitboxActiveDuration);

        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        yield return new WaitForSeconds(postAttackDelay);

        isAttacking = false;
    }
}