using UnityEngine;

// 플레이어 이동, 마우스 방향 조정, 발사체 발사 및 디버그 기즈모를 제어하는 컴포넌트
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public int CurrentAmmo => currentAmmo;
    public int ReserveAmmo => reserveAmmo;
    public bool IsReloading => isReloading;
    // 아이템 등 외부에서 예비 탄약을 보충할 때 호출
    public void AddReserveAmmo(int amount)
    {
        reserveAmmo += amount;
    }

    [Tooltip("체력 관리 컴포넌트")]
    [SerializeField] private HealthSystemForDummies health;

    [Tooltip("직전 프레임의 HP (피격 감지용)")]
    [SerializeField] private float previousHP;

    [Header("이동 속도")]
    [Tooltip("걷기 속도")]
    [SerializeField] private float walkSpeed = 5f;

    [Tooltip("달리기 속도 (Shift)")]
    [SerializeField] private float runSpeed = 9f;

    [Tooltip("현재 달리기 여부")]
    [SerializeField] private bool isRunning;

    [Tooltip("Q/E 키 입력 시 회전속도")]
    [SerializeField] private float rotationSpeed = 5f;

    [Tooltip("이동에 사용되는 Rigidbody 컴포넌트")]
    [SerializeField] private Rigidbody rb;

    [Tooltip("현재 프레임의 입력 방향 (좌우/전후)")]
    [SerializeField] private Vector3 moveInput;

    [Tooltip("현재 플레이어의 Y축 회전 각도")]
    [SerializeField] private float yaw;

    [Header("탄약")]
    [Tooltip("탄창 용량 (1회 장전 시 채워지는 탄약 수)")]
    [SerializeField] private int magazineSize = 12;

    [Tooltip("현재 장전된 탄약 수")]
    [SerializeField] private int currentAmmo;

    [Tooltip("예비 탄약 (탄창 제외 보유 총량)")]
    [SerializeField] private int reserveAmmo = 48;

    [Tooltip("재장전 소요 시간 (초)")]
    [SerializeField] private float reloadDuration = 4f;

    [Tooltip("현재 재장전 중인지 여부")]
    [SerializeField] private bool isReloading;
    [Tooltip("손에 부착된 총기 오브젝트 (사망 시 분리됨)")]
    [SerializeField] private Transform weapon;

    [Tooltip("총기 분리 시 적용할 힘")]
    [SerializeField] private float dropForce = 2f;

    [Tooltip("사망 처리가 이미 실행되었는지 여부 (1회만 실행)")]
    [SerializeField] private bool isDeathHandled;

    [Header("히트스캔")]
    [Tooltip("발사 간격 (초). 값이 작을수록 빠르게 연사")]
    [SerializeField] private float fireRate = 0.25f;

    [Tooltip("다음 발사가 가능해지는 시각")]
    [SerializeField] private float nextFireTime;
    [Tooltip("사거리")]
    [SerializeField] private float hitscanRange = 50f;

    [Tooltip("데미지")]
    [SerializeField] private float hitscanDamage = 20f;

    [Tooltip("피격 판정 대상 레이어")]
    [SerializeField] private LayerMask hitMask = ~0;

    [Tooltip("총알이 발사되는 위치")]
    [SerializeField] private Transform firePoint;

    [Header("총구 이펙트")]
    [Tooltip("Cartoon FX 머즐 플래시 파티클 프리펩 (CFX_SpawnSystem에 사전 등록되어 있어야 함)")]
    [SerializeField] private GameObject muzzleFlashPrefab;

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

    [Tooltip("애니메이션 재생용 Animator 컴포넌트")]
    [SerializeField] private Animator animator;


    private void Awake()
    {
        health = GetComponent<HealthSystemForDummies>();
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        yaw = transform.eulerAngles.y;
        previousHP = health.CurrentHealth;
        currentAmmo = magazineSize;
    }

    private void Update()
    {
        if (!health.IsAlive)
        {
            if (!isDeathHandled)
            {
                animator.SetTrigger("IsDead");
                DropWeapon();
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                isDeathHandled = true;
            }
            return;
        }

        moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
        if (Input.GetKey(KeyCode.Q))
            yaw -= rotationSpeed* Time.deltaTime;
        if (Input.GetKey(KeyCode.E))
            yaw += rotationSpeed* Time.deltaTime;

        isRunning = Input.GetKey(KeyCode.LeftShift) && moveInput.sqrMagnitude > 0.01f;

        animator.SetFloat("MoveX", moveInput.x, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveZ", moveInput.z, 0.1f, Time.deltaTime);
        animator.SetBool("IsRunning", isRunning);

        if (health.CurrentHealth < previousHP)
            animator.SetTrigger("Hit");
        previousHP = health.CurrentHealth;

        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < magazineSize && reserveAmmo > 0)
            StartCoroutine(Reload());

        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextFireTime && !isReloading && currentAmmo > 0)
        {
            Fire();
            animator.SetTrigger("Fire");
            nextFireTime = Time.time + fireRate;
        }
    }
    
    private void FixedUpdate()
    {
        if (!health.IsAlive)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        rb.MoveRotation(Quaternion.Euler(0f, yaw, 0f));

        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        Vector3 vel = transform.TransformDirection(moveInput) * currentSpeed;
        rb.linearVelocity = new Vector3(vel.x, rb.linearVelocity.y, vel.z);
    }
    // firePoint에서 전방으로 Raycast를 쏴 즉시 판정하는 히트스캔 발사
    private void Fire()
    {
        if (firePoint == null) return;

        currentAmmo--;

        SpawnMuzzleFlash();

        if (Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit, hitscanRange, hitMask))
        {
            HealthSystemForDummies health = hit.collider.GetComponentInParent<HealthSystemForDummies>();
            if (health != null)
                health.AddToCurrentHealth(-hitscanDamage);
        }
    }
    // 지정된 시간 동안 재장전 처리 후 탄창을 채움
    private System.Collections.IEnumerator Reload()
    {
        isReloading = true;
        animator.SetTrigger("Reload");

        yield return new WaitForSeconds(reloadDuration);

        int needed = magazineSize - currentAmmo;
        int loaded = Mathf.Min(needed, reserveAmmo);

        currentAmmo += loaded;
        reserveAmmo -= loaded;

        isReloading = false;
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

    // 총기를 손(부모)에서 분리해 독립된 물리 오브젝트로 만듦
    private void DropWeapon()
    {
        if (weapon == null) return;

        weapon.SetParent(null);

        Rigidbody weaponRb = weapon.GetComponent<Rigidbody>();
        if (weaponRb == null)
            weaponRb = weapon.gameObject.AddComponent<Rigidbody>();

        Collider weaponCol = weapon.GetComponent<Collider>();
        if (weaponCol == null)
            weapon.gameObject.AddComponent<BoxCollider>();

        weaponRb.isKinematic = false;
        weaponRb.AddForce(transform.forward * dropForce + Vector3.up * dropForce * 0.5f, ForceMode.Impulse);
        weaponRb.AddTorque(new Vector3(0.123f, 1.176314f, 0.51227f), ForceMode.Impulse);
    }

    private void SpawnMuzzleFlash()
    {
        if (muzzleFlashPrefab == null || firePoint == null) return;

        GameObject flash = Instantiate(muzzleFlashPrefab, firePoint);
        flash.transform.localPosition = Vector3.zero;
        flash.transform.localRotation = Quaternion.identity;
        Destroy(flash, 2f);
    }
}