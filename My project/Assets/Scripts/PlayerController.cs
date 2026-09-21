using UnityEngine;

// 플레이어 이동, 마우스 방향 조정, 발사체 발사 및 디버그 기즈모를 제어하는 컴포넌트
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public int CurrentAmmo => currentAmmo;
    public int ReserveAmmo => reserveAmmo;
    public bool IsReloading => isReloading;
    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    // 아이템 등 외부에서 예비 탄약을 보충할 때 호출
    public void AddReserveAmmo(int amount)
    {
        reserveAmmo += amount;
    }
    [Header("레이저 조준선")]
    [Tooltip("firePoint에서 벽/적까지 그려지는 레이저 LineRenderer")]
    [SerializeField] private LineRenderer laserSight;

    [Tooltip("게임 시작 시 입력 무시 시간 (초). 타이틀 클릭 잔여 입력 방지용")]
    [SerializeField] private float inputIgnoreDuration = 0.2f;

    [Tooltip("입력 무시가 끝나는 시각")]
    [SerializeField] private float inputIgnoreUntil;

    [Tooltip("마우스 좌우 회전 감도")]
    [SerializeField] private float mouseSensitivity = 3f;

    [Tooltip("체력 관리 컴포넌트")]
    [SerializeField] private HealthSystemForDummies health;

    [Tooltip("직전 프레임의 HP (피격 감지용)")]
    [SerializeField] private float previousHP;

    [Header("스태미나")]
    [Tooltip("최대 스태미나")]
    [SerializeField] private float maxStamina = 100f;

    [Tooltip("현재 스태미나")]
    [SerializeField] private float currentStamina;

    [Tooltip("초당 스태미나 소모량 (달리기 중)")]
    [SerializeField] private float staminaDrainRate = 20f;

    [Tooltip("초당 스태미나 회복량 (달리지 않을 때)")]
    [SerializeField] private float staminaRegenRate = 10f;

    [Tooltip("현재 달리기가 스태미나 부족으로 잠겨있는지 여부")]
    [SerializeField] private bool staminaLocked;

    [Header("이동 속도")]
    [Tooltip("걷기 속도")]
    [SerializeField] private float walkSpeed = 5f;

    [Tooltip("달리기 속도 (Shift)")]
    [SerializeField] private float runSpeed = 9f;

    [Tooltip("현재 달리기 여부")]
    [SerializeField] private bool isRunning;

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

    [Header("수류탄")]
    [Tooltip("투척할 수류탄 프리펩")]
    [SerializeField] private GameObject grenadePrefab;

    [Tooltip("수류탄이 발사되는 위치")]
    [SerializeField] private Transform grenadePoint;

    [Tooltip("보유 수류탄 개수")]
    [SerializeField] private int grenadeCount = 2;

    [Tooltip("투척 모션 시작 후 실제로 수류탄이 발사되기까지 지연 시간 (초)")]
    [SerializeField] private float grenadeThrowDelay = 0.3f;

    [Tooltip("투척 쿨다운 (초, 고정)")]
    [SerializeField] private float grenadeCooldown = 2f;

    [Tooltip("다음 투척 가능 시각")]
    [SerializeField] private float nextGrenadeTime;

    [Tooltip("투척 힘 (전방)")]
    [SerializeField] private float throwForce = 12f;

    [Tooltip("투척 힘 (위쪽, 포물선 형성용)")]
    [SerializeField] private float throwUpwardForce = 5f;

    [Header("기즈모 - 투척 궤적")]
    [Tooltip("포물선 궤적 표시 여부")]
    [SerializeField] private bool showThrowTrajectory = true;

    [Tooltip("궤적 샘플링 점 개수")]
    [SerializeField] private int trajectorySteps = 30;

    [Tooltip("궤적 시뮬레이션 시간 간격")]
    [SerializeField] private float trajectoryTimeStep = 0.1f;

    // 외부(GrenadeUI)에서 참조
    public int GrenadeCount => grenadeCount;

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

        currentStamina = maxStamina;

        inputIgnoreUntil = Time.time + inputIgnoreDuration;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

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


        isRunning = Input.GetKey(KeyCode.LeftShift) && moveInput.sqrMagnitude > 0.01f;

        // 마우스 방향 회전 방식 (복원)
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;

        bool wantsToRun = Input.GetKey(KeyCode.LeftShift) && moveInput.sqrMagnitude > 0.01f && !isReloading;

        if (staminaLocked)
        {
            isRunning = false;
            if (currentStamina >= maxStamina * 0.3f)
                staminaLocked = false;
        }
        else
        {
            isRunning = wantsToRun && currentStamina > 0f;
        }

        if (isRunning)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isRunning = false;
                staminaLocked = true;
            }
        }
        else
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);
        }


        animator.SetFloat("MoveX", moveInput.x, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveZ", moveInput.z, 0.1f, Time.deltaTime);
        animator.SetBool("IsRunning", isRunning);

        if (health.CurrentHealth < previousHP)
            animator.SetTrigger("Hit");
        previousHP = health.CurrentHealth;

        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < magazineSize && reserveAmmo > 0)
            StartCoroutine(Reload());


        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextFireTime && !isReloading && currentAmmo > 0)

        if (Input.GetKeyDown(KeyCode.E) && Time.time >= nextGrenadeTime && grenadeCount > 0)
        {
            grenadeCount--;
            nextGrenadeTime = Time.time + grenadeCooldown;
            animator.SetTrigger("ThrowGrenade");
            StartCoroutine(ThrowGrenadeAfterDelay());
        }

        if (Time.time >= inputIgnoreUntil && Input.GetButton("Fire1") && Time.time >= nextFireTime && !isReloading && currentAmmo > 0)
        {
            Fire();
            animator.SetTrigger("Fire");
           nextFireTime = Time.time + fireRate;
        }
    }
    private void LateUpdate()
    {
        UpdateLaserSight();
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

            HitFlash flash = hit.collider.GetComponentInParent<HitFlash>();
            if (flash != null)
                flash.Flash();
        }
    }
    // firePoint에서 전방으로 Raycast를 1회 수행해 레이저 도착 지점만 반환 (판정용 아님)
    private Vector3 GetHitscanEndPoint()
    {
        if (Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit, hitscanRange, hitMask))
            return hit.point;

        return firePoint.position + firePoint.forward * hitscanRange;
    }
    private void UpdateLaserSight()
    {
        if (laserSight == null || firePoint == null) return;

        Vector3 endPoint = GetHitscanEndPoint();

        laserSight.SetPosition(0, firePoint.position);
        laserSight.SetPosition(1, endPoint);
    }
    // 투척 모션 시작 후 grenadeThrowDelay 경과 시 실제 수류탄 생성 및 발사
    private System.Collections.IEnumerator ThrowGrenadeAfterDelay()
    {
        yield return new WaitForSeconds(grenadeThrowDelay);

        if (grenadePrefab == null || grenadePoint == null) yield break;

        GameObject grenade = Instantiate(grenadePrefab, grenadePoint.position, Quaternion.identity);
        Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();
        if (grenadeRb != null)
            grenadeRb.AddForce(grenadePoint.forward * throwForce + Vector3.up * throwUpwardForce, ForceMode.Impulse);
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

        // 수류탄 포물선 투척 궤적 표시
        if (showThrowTrajectory && firePoint != null)
        {
            Gizmos.color = Color.cyan;

            Rigidbody tempRb = GetComponent<Rigidbody>();
            float mass = tempRb != null ? 1f : 1f; // Impulse 기준이므로 질량 1 가정 (수류탄 프리펩의 실제 Rigidbody mass와 다를 경우 궤적이 다소 어긋날 수 있음)

            Vector3 startPos = firePoint.position;
            Vector3 velocity = firePoint.forward * throwForce + Vector3.up * throwUpwardForce;
            Vector3 gravity = Physics.gravity;

            Vector3 prevPoint = startPos;
            for (int i = 1; i <= trajectorySteps; i++)
            {
                float t = i * trajectoryTimeStep;
                Vector3 point = startPos + velocity * t + 0.5f * gravity * t * t;
                Gizmos.DrawLine(prevPoint, point);
                prevPoint = point;

                if (point.y < startPos.y - 10f) break; // 바닥 아래로 과도하게 내려가면 중단
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