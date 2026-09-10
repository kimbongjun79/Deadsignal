using UnityEngine;

// 활성화된 동안 Player와 1회만 충돌 데미지를 입히는 공격 판정
[RequireComponent(typeof(Collider))]
public class AttackHitbox : MonoBehaviour
{
    [Tooltip("공격 데미지")]
    [SerializeField] private float damage = 10f;

    [Tooltip("이번 활성화 구간에서 이미 타격했는지 여부")]
    [SerializeField] private bool hasHit;

    // 히트박스 활성화 직전 호출해 타격 여부 초기화
    public void ResetHit()
    {
        hasHit = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit || !other.CompareTag("Player")) return;

        HealthSystemForDummies health = other.GetComponentInParent<HealthSystemForDummies>();
        if (health != null)
        {
            health.AddToCurrentHealth(-damage);
            hasHit = true;
        }
    }
}