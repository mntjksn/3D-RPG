using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;

[RequireComponent(typeof(CharacterController))]
// 플레이어 이동, 중력 적용, 발소리 재생 처리
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
        // 원격 플레이어는 중력만 적용
        if (!photonView.IsMine)
        {
            verticalVelocity += gravity * Time.deltaTime;
            return;
        }

        float x = 0f, z = 0f;

        bool chatOpen = EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject != null &&
            EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null;
        if (!chatOpen && (actionLock == null || actionLock.CanMove))
        {
            x = Input.GetAxis("Horizontal");
            z = Input.GetAxis("Vertical");
        }

        Vector3 inputDir = transform.right * x + transform.forward * z;
        if (inputDir.magnitude > 1f)
            inputDir.Normalize();

        // 공격/방어 중에는 이동 속도 제한
        float targetSpeed = inputDir.magnitude * playerStat.GetSpeed();
        if (actionLock != null && actionLock.IsAttacking)
            targetSpeed = inputDir.magnitude > 0.01f ? attackMoveSpeed : 0f;
        else if (actionLock != null && actionLock.IsShielding)
            targetSpeed = inputDir.magnitude > 0.01f ? shieldMoveSpeed : 0f;

        float rate = targetSpeed > currentSpeed ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.deltaTime);

        Vector3 move = inputDir.normalized * currentSpeed;
        playerAnimation?.SetMoveSpeed(currentSpeed);

        // 착지 시 낙하 속도 리셋 (튕김 방지)
        if (characterController.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        characterController.Move(move * Time.deltaTime);
        HandleFootstepSound(inputDir);
    }

    // 이동 속도에 따라 발소리 재생 간격을 동적으로 조절
    private void HandleFootstepSound(Vector3 inputDir)
    {
        // 공격/방어 중에는 발소리 타이머 리셋
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
            // 너무 짧은 간격으로 중복 재생 방지
            if (Time.time - lastFootstepTime < 0.1f) return;

            SoundManager.Instance?.PlaySFX(SfxType.Footstep);
            lastFootstepTime = Time.time;

            // 속도가 빠를수록 발소리 간격 짧아짐
            float maxSpeed = Mathf.Max(playerStat.GetSpeed(), 0.01f);
            float speedRatio = Mathf.Clamp01(currentSpeed / maxSpeed);
            footstepTimer = Mathf.Lerp(maxFootstepInterval, minFootstepInterval, speedRatio);
        }
    }
}