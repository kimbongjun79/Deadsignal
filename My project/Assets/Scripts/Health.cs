using UnityEngine;
using UnityEngine.Events;

// 오브젝트의 체력을 관리하는 컴포넌트
public class Health : MonoBehaviour
{
    [Tooltip("현재 체력")]
    public int HP = 100;

    [Tooltip("피격 시 호출되는 이벤트")]
    public UnityEvent OnDamaged;

    [Tooltip("사망 시 호출되는 이벤트 (HP가 0 이하가 된 최초 시점)")]
    public UnityEvent OnDeath;

    private bool isDead;

    // 데미지 적용 및 이벤트 호출
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        HP -= damage;
        OnDamaged?.Invoke();

        if (HP <= 0)
        {
            isDead = true;
            OnDeath?.Invoke();
        }
    }
}