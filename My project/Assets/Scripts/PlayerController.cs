using UnityEngine;

// 플레이어 이동, 마우스 방향 조정, 발사체 발사 및 디버그 기즈모를 제어하는 컴포넌트
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Tooltip("체력 관리 컴포넌트")]
    [SerializeField] private Health health;
    [Tooltip("플레이어 이동 속도")]
    [SerializeField] private float moveSpeed = 5f;

    [Tooltip("마우스 좌우 회전 감도")]
    [SerializeField] private float mouseSensitivity = 3f;

    [Tooltip("이동에 사용되는 Rigidbody 컴포넌트")]
    [SerializeField] private Rigidbody rb;

    [Tooltip("현재 프레임의 입력 방향 (좌우/전후)")]
    [SerializeField] private Vector3 moveInput;

    [Tooltip("현재 플레이어의 Y축 회전 각도")]
    [SerializeField] private float yaw;

    [Header("발사체")]
    [Tooltip("발사할 총알 프리펩")]
    [SerializeField] private GameObject bulletPrefab;

    [Tooltip("총알이 발사되는 위치")]
    [SerializeField] private Transform firePoint;

    [Tooltip("발사체 속도")]
    [SerializeField] private float bulletSpeed = 20f;

    [Header("기즈모 - 시야각")]
    [Tooltip("시야각 표시 여부")]
    [SerializeField] private bool showViewAngleGizmo = true;

    [Tooltip("시야각 (도 단위)")]
    [SerializeField] private float viewAngle = 60f;

    [Tooltip("시야각 표시 반경")]
    [SerializeField] private float viewDistance = 3f;

    [Header("기즈모 - 방향 화살표")]
    [Tooltip("전방/이동 방향 화살표 표시 여부")]
    [SerializeField] private bool showDirectionArrows = true;

    [Tooltip("화살표 길이")]
    [SerializeField] private float arrowLength = 2f;

    private void Awake()
    {
        health = GetComponent<Health>();
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        yaw = transform.eulerAngles.y;
    }

    private void Update()
    {
        moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;

        if (Input.GetKey(KeyCode.Space))
            Fire();

        if (health != null && health.HP <= 0)
            gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        rb.MoveRotation(Quaternion.Euler(0f, yaw, 0f));

        Vector3 vel = transform.TransformDirection(moveInput) * moveSpeed;
        rb.linearVelocity = new Vector3(vel.x, rb.linearVelocity.y, vel.z);
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

        // 시야각 표시 (부채꼴)
        if (showViewAngleGizmo)
        {
            Gizmos.color = Color.yellow;
            Quaternion leftRot = Quaternion.Euler(0f, -viewAngle / 2f, 0f);
            Quaternion rightRot = Quaternion.Euler(0f, viewAngle / 2f, 0f);
            Vector3 leftDir = leftRot * transform.forward * viewDistance;
            Vector3 rightDir = rightRot * transform.forward * viewDistance;

            Gizmos.DrawLine(origin, origin + leftDir);
            Gizmos.DrawLine(origin, origin + rightDir);

            int segments = 20;
            Vector3 prevPoint = origin + leftDir;
            for (int i = 1; i <= segments; i++)
            {
                float angle = -viewAngle / 2f + (viewAngle * i / segments);
                Vector3 point = origin + Quaternion.Euler(0f, angle, 0f) * transform.forward * viewDistance;
                Gizmos.DrawLine(prevPoint, point);
                prevPoint = point;
            }
        }

        // 전방/이동 방향 화살표 표시
        if (showDirectionArrows)
        {
            Gizmos.color = Color.blue;
            DrawArrow(origin, transform.forward * arrowLength);

            if (moveInput.sqrMagnitude > 0.01f)
            {
                Gizmos.color = Color.red;
                Vector3 moveDir = transform.TransformDirection(moveInput);
                DrawArrow(origin, moveDir * arrowLength);
            }
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