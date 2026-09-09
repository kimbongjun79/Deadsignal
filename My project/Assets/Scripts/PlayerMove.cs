using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 단일 스크립트 플레이어 컨트롤러
///
/// 기능
/// 1. WASD 이동
/// 2. Inspector에서 이동 속도 조절
/// 3. 마우스 월드 위치를 추적하여 플레이어 방향 변경
/// 4. 플레이어 회전 속도 조절
/// 5. 카메라 Follow Delay
/// 6. 카메라 Rotation Delay
/// 7. Scene View 개발용 Gizmo
///    - 플레이어 바라보는 방향
///    - 마우스 조준 방향
///    - 시야각(FOV)
///    - 이동 방향
/// </summary>
public class PlayerMove : MonoBehaviour
{
    // =========================================================
    // 이동 설정
    // =========================================================

    [Header("=== Movement ===")]

    [Tooltip("플레이어 이동 속도")]
    [SerializeField]
    private float moveSpeed = 5f;

    [Tooltip("중력")]
    [SerializeField]
    private float gravity = -20f;

    [Tooltip("공중에 있을 때 이동")]
    [SerializeField]
    private bool allowAirMovement = true;


    // =========================================================
    // 마우스 방향 설정
    // =========================================================

    [Header("=== Mouse Tracking ===")]

    [Tooltip("마우스를 이용해 플레이어 방향을 변경할지 여부")]
    [SerializeField]
    private bool useMouseRotation = true;

    [Tooltip("플레이어가 마우스 방향으로 회전하는 속도")]
    [SerializeField]
    private float rotationSpeed = 15f;

    [Tooltip("마우스 Raycast용 바닥 Layer")]
    [SerializeField]
    private LayerMask mouseGroundMask = ~0;

    [Tooltip("플레이어가 바라보는 시야각")]
    [Range(1f, 360f)]
    [SerializeField]
    private float viewAngle = 120f;

    [Tooltip("시야 표시 거리")]
    [SerializeField]
    private float viewDistance = 5f;


    // =========================================================
    // 카메라 설정
    // =========================================================

    [Header("=== Camera ===")]

    [Tooltip("추적할 카메라. 비워두면 Main Camera 사용")]
    [SerializeField]
    private Camera targetCamera;

    [Tooltip("플레이어와 카메라 사이의 거리")]
    [SerializeField]
    private float cameraDistance = 8f;

    [Tooltip("플레이어보다 카메라가 높은 정도")]
    [SerializeField]
    private float cameraHeight = 6f;

    [Tooltip("카메라 위치 추적 딜레이. 값이 클수록 느리게 따라옴")]
    [SerializeField]
    private float cameraFollowDelay = 0.15f;

    [Tooltip("카메라 회전 추적 딜레이")]
    [SerializeField]
    private float cameraRotationDelay = 0.1f;

    [Tooltip("카메라가 플레이어를 바라보는 높이")]
    [SerializeField]
    private float cameraLookHeight = 1f;

    [Tooltip("카메라가 플레이어 방향을 따라갈지 여부")]
    [SerializeField]
    private bool cameraFollowPlayerRotation = true;


    // =========================================================
    // 개발용 GUI / Gizmo
    // =========================================================

    [Header("=== Scene View Debug ===")]

    [Tooltip("Scene View에 시야각 표시")]
    [SerializeField]
    private bool showViewAngle = true;

    [Tooltip("Scene View에 바라보는 방향 화살표 표시")]
    [SerializeField]
    private bool showForwardArrow = true;

    [Tooltip("Scene View에 마우스 조준 방향 표시")]
    [SerializeField]
    private bool showMouseDirection = true;

    [Tooltip("Scene View에 이동 방향 표시")]
    [SerializeField]
    private bool showMovementDirection = true;

    [Tooltip("Gizmo 색상")]
    [SerializeField]
    private Color viewColor = new Color(0f, 1f, 0f, 0.15f);

    [Tooltip("플레이어 방향 화살표 색상")]
    [SerializeField]
    private Color forwardColor = Color.blue;

    [Tooltip("마우스 방향 색상")]
    [SerializeField]
    private Color mouseColor = Color.red;

    [Tooltip("이동 방향 색상")]
    [SerializeField]
    private Color movementColor = Color.yellow;


    // =========================================================
    // 내부 변수
    // =========================================================

    private CharacterController characterController;

    private Vector3 velocity;
    private Vector3 currentMoveDirection;

    private Vector3 mouseWorldPosition;
    private bool hasMousePosition;


    // 카메라 SmoothDamp용 변수
    private Vector3 cameraVelocity;

    // 카메라 회전 SmoothDamp용 변수
    private float cameraYawVelocity;
    private float currentCameraYaw;


    // =========================================================
    // Unity
    // =========================================================

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera != null)
        {
            currentCameraYaw = targetCamera.transform.eulerAngles.y;
        }
    }


    private void Update()
    {
        HandleMovement();

        if (useMouseRotation)
        {
            HandleMouseRotation();
        }

        HandleGravity();
    }


    private void LateUpdate()
    {
        HandleCamera();
    }


    // =========================================================
    // WASD 이동
    // =========================================================

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical);

        inputDirection = Vector3.ClampMagnitude(inputDirection, 1f);

        if (inputDirection.sqrMagnitude <= 0.001f)
        {
            currentMoveDirection = Vector3.zero;
            return;
        }


        // 카메라 기준 WASD 이동
        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        if (targetCamera != null)
        {
            forward = targetCamera.transform.forward;
            right = targetCamera.transform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();
        }


        Vector3 moveDirection =
            forward * inputDirection.z +
            right * inputDirection.x;

        moveDirection.y = 0f;

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        currentMoveDirection = moveDirection;


        if (characterController.isGrounded || allowAirMovement)
        {
            characterController.Move(
                moveDirection * moveSpeed * Time.deltaTime
            );
        }
    }


    // =========================================================
    // 마우스 위치 추적
    // =========================================================

    private void HandleMouseRotation()
    {
        if (targetCamera == null)
            return;

        // 마우스 위치에서 카메라 방향으로 Ray 발사
        Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);

        // 플레이어 높이를 기준으로 한 수평면
        Plane groundPlane = new Plane(
            Vector3.up,
            transform.position
        );

        float distance;

        if (groundPlane.Raycast(ray, out distance))
        {
            // 마우스가 가리키는 월드 좌표
            mouseWorldPosition = ray.GetPoint(distance);

            hasMousePosition = true;

            // 캐릭터 -> 마우스 방향
            Vector3 direction =
                mouseWorldPosition - transform.position;

            // 수평 회전만 사용
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                direction.Normalize();

                // ★ 캐릭터의 방향 = 마우스 방향
                Quaternion targetRotation =
                    Quaternion.LookRotation(direction);

                // 부드러운 회전
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }



    // =========================================================
    // 중력
    // =========================================================

    private void HandleGravity()
    {
        if (characterController.isGrounded)
        {
            if (velocity.y < 0f)
            {
                velocity.y = -2f;
            }
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }


        characterController.Move(
            velocity * Time.deltaTime
        );
    }


    // =========================================================
    // 카메라
    // =========================================================

    private void HandleCamera()
    {
        if (targetCamera == null)
        {
            return;
        }


        // -----------------------------------------------------
        // 카메라 위치
        // -----------------------------------------------------

        Vector3 cameraDirection = transform.forward;

        if (!cameraFollowPlayerRotation)
        {
            cameraDirection = Vector3.forward;
        }

        cameraDirection.y = 0f;

        if (cameraDirection.sqrMagnitude < 0.001f)
        {
            cameraDirection = Vector3.forward;
        }

        cameraDirection.Normalize();


        Vector3 desiredCameraPosition =
            transform.position
            - cameraDirection * cameraDistance
            + Vector3.up * cameraHeight;


        // SmoothDamp를 이용한 카메라 딜레이
        float followSmoothTime =
            Mathf.Max(0.001f, cameraFollowDelay);

        targetCamera.transform.position =
            Vector3.SmoothDamp(
                targetCamera.transform.position,
                desiredCameraPosition,
                ref cameraVelocity,
                followSmoothTime
            );


        // -----------------------------------------------------
        // 카메라 회전
        // -----------------------------------------------------

        Vector3 lookTarget =
            transform.position +
            Vector3.up * cameraLookHeight;


        Vector3 lookDirection =
            lookTarget - targetCamera.transform.position;


        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(lookDirection);


            // 회전 딜레이
            float targetYaw = targetRotation.eulerAngles.y;

            currentCameraYaw = Mathf.SmoothDampAngle(
                currentCameraYaw,
                targetYaw,
                ref cameraYawVelocity,
                Mathf.Max(0.001f, cameraRotationDelay)
            );


            Vector3 currentEuler =
                targetCamera.transform.eulerAngles;

            currentEuler.y = currentCameraYaw;

            // X축은 LookAt 계산을 사용
            Quaternion finalRotation =
                Quaternion.LookRotation(
                    lookTarget - targetCamera.transform.position
                );

            Vector3 finalEuler =
                finalRotation.eulerAngles;

            finalEuler.y = currentCameraYaw;

            targetCamera.transform.rotation =
                Quaternion.Euler(
                    finalEuler.x,
                    currentCameraYaw,
                    0f
                );
        }
    }


    // =========================================================
    // Scene View Gizmo
    // =========================================================

    private void OnDrawGizmos()
    {
        Vector3 position = transform.position;

        // -----------------------------------------------------
        // 플레이어 Forward
        // -----------------------------------------------------

        if (showForwardArrow)
        {
            DrawArrow(
                position + Vector3.up * 0.1f,
                transform.forward,
                viewDistance,
                forwardColor
            );
        }


        // -----------------------------------------------------
        // 마우스 방향
        // -----------------------------------------------------

        if (showMouseDirection && hasMousePosition)
        {
            Vector3 mouseDirection =
                mouseWorldPosition - position;

            mouseDirection.y = 0f;

            if (mouseDirection.sqrMagnitude > 0.001f)
            {
                DrawArrow(
                    position + Vector3.up * 0.15f,
                    mouseDirection.normalized,
                    Mathf.Min(
                        viewDistance,
                        mouseDirection.magnitude
                    ),
                    mouseColor
                );

                Gizmos.color = mouseColor;
                Gizmos.DrawSphere(
                    mouseWorldPosition,
                    0.12f
                );
            }
        }


        // -----------------------------------------------------
        // 이동 방향
        // -----------------------------------------------------

        if (showMovementDirection &&
            currentMoveDirection.sqrMagnitude > 0.001f)
        {
            DrawArrow(
                position + Vector3.up * 0.2f,
                currentMoveDirection,
                1.5f,
                movementColor
            );
        }


        // -----------------------------------------------------
        // 시야각
        // -----------------------------------------------------

        if (showViewAngle)
        {
            DrawViewCone();
        }
    }


    // =========================================================
    // 시야각 표시
    // =========================================================

    private void DrawViewCone()
    {
        Vector3 origin =
            transform.position + Vector3.up * 0.05f;

        Vector3 forward =
            transform.forward;

        float halfAngle =
            viewAngle * 0.5f;


        // 좌 / 우 시야 끝
        Vector3 leftDirection =
            Quaternion.Euler(
                0f,
                -halfAngle,
                0f
            ) * forward;

        Vector3 rightDirection =
            Quaternion.Euler(
                0f,
                halfAngle,
                0f
            ) * forward;


        Gizmos.color = viewColor;


        // 좌우 경계선
        Gizmos.DrawLine(
            origin,
            origin + leftDirection * viewDistance
        );

        Gizmos.DrawLine(
            origin,
            origin + rightDirection * viewDistance
        );


        // 시야각 Arc
        int segments = 30;

        Vector3 previousPoint =
            origin + leftDirection * viewDistance;


        for (int i = 1; i <= segments; i++)
        {
            float angle =
                -halfAngle +
                (viewAngle / segments) * i;


            Vector3 direction =
                Quaternion.Euler(
                    0f,
                    angle,
                    0f
                ) * forward;


            Vector3 currentPoint =
                origin + direction * viewDistance;


            Gizmos.DrawLine(
                previousPoint,
                currentPoint
            );


            previousPoint = currentPoint;
        }


        // 시야 부채꼴 내부 보조선
        int guideLines = 5;

        for (int i = 1; i < guideLines; i++)
        {
            float angle =
                -halfAngle +
                (viewAngle / guideLines) * i;


            Vector3 direction =
                Quaternion.Euler(
                    0f,
                    angle,
                    0f
                ) * forward;


            Gizmos.DrawLine(
                origin,
                origin + direction * viewDistance
            );
        }
    }


    // =========================================================
    // 화살표
    // =========================================================

    private void DrawArrow(
        Vector3 start,
        Vector3 direction,
        float length,
        Color color)
    {
        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        direction.Normalize();

        Gizmos.color = color;

        Vector3 end =
            start + direction * length;

        Gizmos.DrawLine(
            start,
            end
        );


        // 화살표 머리
        Vector3 right =
            Quaternion.Euler(
                0f,
                150f,
                0f
            ) * direction;

        Vector3 left =
            Quaternion.Euler(
                0f,
                -150f,
                0f
            ) * direction;


        float arrowSize =
            Mathf.Min(length * 0.2f, 0.5f);


        Gizmos.DrawLine(
            end,
            end + right * arrowSize
        );

        Gizmos.DrawLine(
            end,
            end + left * arrowSize
        );
    }


    // =========================================================
    // Inspector에서 값이 이상해지지 않도록 제한
    // =========================================================

    private void OnValidate()
    {
        moveSpeed =
            Mathf.Max(0f, moveSpeed);

        rotationSpeed =
            Mathf.Max(0f, rotationSpeed);

        cameraDistance =
            Mathf.Max(0.1f, cameraDistance);

        cameraHeight =
            Mathf.Max(0f, cameraHeight);

        cameraFollowDelay =
            Mathf.Max(0.001f, cameraFollowDelay);

        cameraRotationDelay =
            Mathf.Max(0.001f, cameraRotationDelay);

        viewDistance =
            Mathf.Max(0.1f, viewDistance);
    }
}


