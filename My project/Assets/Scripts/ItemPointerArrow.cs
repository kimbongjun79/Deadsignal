using UnityEngine;

// 플레이어 발밑에서 가장 가까운 아이템(일정 반경 내) 방향을 가리키는 화살표
public class ItemPointerArrow : MonoBehaviour
{
    [Tooltip("화살표를 표시할 오브젝트 (플레이어 발밑에 배치된 화살표 메쉬/스프라이트)")]
    [SerializeField] private GameObject arrowObject;

    [Tooltip("아이템 탐색 반경")]
    [SerializeField] private float detectionRadius = 15f;

    [Tooltip("아이템 오브젝트가 가진 레이어 (탐색 필터링용)")]
    [SerializeField] private LayerMask itemLayer;

    [Tooltip("화살표 갱신 주기 (초). 값이 클수록 연산 부담 감소")]
    [SerializeField] private float updateInterval = 0.2f;

    private float updateTimer;

    private void Update()
    {
        updateTimer -= Time.deltaTime;
        if (updateTimer > 0f) return;
        updateTimer = updateInterval;

        UpdatePointer();
    }

    // 반경 내 가장 가까운 아이템을 찾아 화살표 방향을 갱신
    private void UpdatePointer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, itemLayer);

        if (hits.Length == 0)
        {
            arrowObject.SetActive(false);
            return;
        }

        Transform nearest = null;
        float nearestDist = float.MaxValue;

        foreach (Collider col in hits)
        {
            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = col.transform;
            }
        }

        if (nearest == null)
        {
            arrowObject.SetActive(false);
            return;
        }

        arrowObject.SetActive(true);

        Vector3 direction = nearest.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            arrowObject.transform.rotation = targetRotation;
        }
    }
}