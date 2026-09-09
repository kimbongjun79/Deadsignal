using UnityEngine;

// 총알 발사체의 충돌 처리를 담당하는 컴포넌트
public class Bullet : MonoBehaviour
{
    [Tooltip("총알 피격 시 대상에게 가하는 데미지")]
    [SerializeField] private int damage = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            Health health = other.GetComponentInParent<Health>();
            if (health != null)
                health.TakeDamage(damage);
        }

        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}