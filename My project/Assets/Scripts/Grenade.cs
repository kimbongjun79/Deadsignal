using UnityEngine;

// 물리 비행 후 퓨즈 타이머 경과 시 폭발, 반경 내 전원(플레이어 포함) 고정 데미지
[RequireComponent(typeof(Rigidbody))]
public class Grenade : MonoBehaviour
{
    [Tooltip("던져진 후 폭발까지 대기 시간 (초)")]
    [SerializeField] private float fuseTime = 3f;

    [Tooltip("폭발 반경")]
    [SerializeField] private float explosionRadius = 5f;

    [Tooltip("폭발 고정 데미지 (거리 감쇠 없음, 플레이어 포함 전원 동일)")]
    [SerializeField] private float explosionDamage = 60f;

    [Tooltip("피격 판정 대상 레이어 (Player 포함)")]
    [SerializeField] private LayerMask hitMask = ~0;

    [Tooltip("폭발 이펙트 프리펩")]
    [SerializeField] private GameObject explosionEffectPrefab;

    private float fuseTimer;
    private bool exploded;

    private void Awake()
    {
        fuseTimer = fuseTime;
    }

    private void Update()
    {
        fuseTimer -= Time.deltaTime;
        if (fuseTimer <= 0f && !exploded)
            Explode();
    }

    // 반경 내 HealthSystemForDummies를 가진 모든 대상(플레이어 포함)에게 고정 데미지 적용
    private void Explode()
    {
        exploded = true;

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, hitMask);
        foreach (Collider col in hits)
        {
            HealthSystemForDummies health = col.GetComponentInParent<HealthSystemForDummies>();
            if (health == null) continue;

            health.AddToCurrentHealth(-explosionDamage);

            HitFlash flash = col.GetComponentInParent<HitFlash>();
            if (flash != null)
                flash.Flash();
        }

        if (explosionEffectPrefab != null)
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    // 프리펩 선택 시 폭발 반경 표시
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}