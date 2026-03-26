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

        // 가속 / 감속 처리
        if (targetSpeed > currentSpeed)
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        else
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, deceleration * Time.deltaTime);

        Vector3 move = inputDir.normalized * currentSpeed;

        animator.SetFloat("Speed", currentSpeed);

        if (characterController.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        characterController.Move(move * Time.deltaTime);
    }
}