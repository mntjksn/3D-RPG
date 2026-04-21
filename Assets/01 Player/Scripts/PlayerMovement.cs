using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviourPun
{
    [Header("Move")]
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;
    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;
    [Header("Attack Move")]
    [SerializeField] private float attackMoveSpeed = 2f;
    [SerializeField] private float shieldMoveSpeed = 0.5f;
    [Header("Footstep")]
    [SerializeField] private float minFootstepInterval = 0.3f;
    [SerializeField] private float maxFootstepInterval = 0.6f;

    private CharacterController characterController;
    private PlayerAnimation playerAnimation;
    private PlayerActionLock actionLock;
    private PlayerStat playerStat;
    private float verticalVelocity;
    private float currentSpeed;
    private float footstepTimer = 0.1f;
    private float lastFootstepTime;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerAnimation = GetComponent<PlayerAnimation>();
        actionLock = GetComponent<PlayerActionLock>();
        playerStat = GetComponent<PlayerStat>();
    }

    private void Update()
    {
        // 내 플레이어만 입력 처리
        if (!photonView.IsMine)
        {
            // 다른 플레이어는 중력만 적용
            verticalVelocity += gravity * Time.deltaTime;
            return;
        }

        float x = 0f;
        float z = 0f;

        if (actionLock == null || actionLock.CanMove)
        {
            x = Input.GetAxis("Horizontal");
            z = Input.GetAxis("Vertical");
        }

        Vector3 inputDir = transform.right * x + transform.forward * z;

        if (inputDir.magnitude > 1f)
            inputDir.Normalize();

        float targetSpeed = inputDir.magnitude * playerStat.GetSpeed();

        if (actionLock != null && actionLock.IsAttacking)
            targetSpeed = inputDir.magnitude > 0.01f ? attackMoveSpeed : 0f;
        else if (actionLock != null && actionLock.IsShielding)
            targetSpeed = inputDir.magnitude > 0.01f ? shieldMoveSpeed : 0f;

        float rate = (targetSpeed > currentSpeed) ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.deltaTime);

        Vector3 move = inputDir.normalized * currentSpeed;
        playerAnimation?.SetMoveSpeed(currentSpeed);

        if (characterController.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;
        characterController.Move(move * Time.deltaTime);
        HandleFootstepSound(inputDir);
    }

    private void HandleFootstepSound(Vector3 inputDir)
    {
        if (actionLock != null && (actionLock.IsAttacking || actionLock.IsShielding))
        {
            footstepTimer = 0f;
            return;
        }

        bool isMoving = inputDir.magnitude > 0.1f && characterController.isGrounded && currentSpeed > 0.1f;

        if (!isMoving)
        {
            footstepTimer = 0f;
            return;
        }

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0f)
        {
            if (Time.time - lastFootstepTime < 0.1f)
                return;

            SoundManager.Instance.PlaySFX(SfxType.Footstep);
            lastFootstepTime = Time.time;

            float maxSpeed = Mathf.Max(playerStat.GetSpeed(), 0.01f);
            float speedRatio = Mathf.Clamp01(currentSpeed / maxSpeed);
            float interval = Mathf.Lerp(maxFootstepInterval, minFootstepInterval, speedRatio);
            footstepTimer = interval;
        }
    }
}