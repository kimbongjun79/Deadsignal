using UnityEngine;

// 플레이어와 접촉 시 효과를 적용하는 아이템 (회복/탄약)
public class Item : MonoBehaviour
{
    public enum ItemType { Health, Ammo }

    [Tooltip("아이템 종류")]
    [SerializeField] private ItemType itemType;

    [Tooltip("회복량 (Health 타입일 때 사용)")]
    [SerializeField] private float healAmount = 30f;

    [Tooltip("보충 탄약량 (Ammo 타입일 때 사용)")]
    [SerializeField] private int ammoAmount = 24;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (itemType == ItemType.Health)
        {
            HealthSystemForDummies health = other.GetComponentInParent<HealthSystemForDummies>();
            if (health != null)
                health.AddToCurrentHealth(healAmount);
        }
        else if (itemType == ItemType.Ammo)
        {
            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player != null)
                player.AddReserveAmmo(ammoAmount);
        }

        Destroy(gameObject);
    }
}