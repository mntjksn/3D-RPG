using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 10f;   // 가속도
    [SerializeField] private float deceleration = 15f;   // 감속도

    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;

    [Header("Attack Move")]
    [SerializeField] private float attackMoveSpeed = 2f;

    private CharacterController characterController;
    private Animator animator;
    private PlayerActionLock actionLock;

    private float verticalVelocity;
    private float currentSpeed;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        actionLock = GetComponent<PlayerActionLock>();
    }

    private void Update()
    {
        float x = 0f;
        float z = 0f;

        if (actionLock == null || actionLock.CanMove)
        {
            x = Input.GetAxis("Horizontal");
            z = Input.GetAxis("Vertical");
        }

        Vector3 inputDir = transform.right * x + transform.forward * z;

        // 대각선 이동 속도 보정
        if (inputDir.magnitude > 1f)
            inputDir.Normalize();

        float targetSpeed = inputDir.magnitude * moveSpeed;

        if (actionLock != null && actionLock.IsAttacking)
        {
            if (inputDir.magnitude > 0.01f)
                targetSpeed = attackMoveSpeed;
            else
                targetSpeed = 0f;
        }

        // 가속 / 감속 처리
        float rate = (targetSpeed > currentSpeed) ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.deltaTime);

        Vector3 move = inputDir.normalized * currentSpeed;

        animator.SetFloat("Speed", currentSpeed, 0.1f, Time.deltaTime);

        if (characterController.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        characterController.Move(move * Time.deltaTime);
    }
}