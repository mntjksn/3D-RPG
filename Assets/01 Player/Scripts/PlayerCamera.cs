using UnityEngine;
using Photon.Pun;

// 3인칭 카메라 - 마우스 회전, 상하 각도 보간, 벽 충돌 시 거리 조정
public class PlayerCamera : MonoBehaviourPun
{
    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform cameraTransform;

    [Header("Sensitivity")]
    [SerializeField] private float mouseSensitivity = 200f;

    [Header("Rotation")]
    [SerializeField] private float minXRotation = -20f;
    [SerializeField] private float maxXRotation = 60f;
    [SerializeField] private float cameraSmoothSpeed = 10f;

    [Header("Distance")]
    [SerializeField] private float cameraDistance = 4f;
    [SerializeField] private float minCameraDistance = 0.5f;

    [Header("Collision")]
    [SerializeField] private float collisionRadius = 0.2f;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float collisionOffset = 0.1f;
    [SerializeField] private float distanceSmoothSpeed = 15f;

    private float xRotation = 20f;
    private float currentXRotation = 20f;
    private float currentCameraDistance;

    private PlayerActionLock actionLock;

    private void Start()
    {
        // 내 플레이어가 아니면 카메라 비활성화
        if (!photonView.IsMine)
        {
            cameraTransform?.gameObject.SetActive(false);
            enabled = false;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (player != null)
            actionLock = player.GetComponent<PlayerActionLock>();

        currentXRotation = xRotation;
        currentCameraDistance = cameraDistance;
    }

    // 마우스 입력으로 카메라 상하 회전 및 플레이어 좌우 회전
    private void Update()
    {
        if (!photonView.IsMine) return;

        if (actionLock == null || actionLock.CanLook)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation = Mathf.Clamp(xRotation - mouseY, minXRotation, maxXRotation);

            player?.Rotate(Vector3.up * mouseX);
        }

        currentXRotation = Mathf.Lerp(currentXRotation, xRotation, cameraSmoothSpeed * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(currentXRotation, 0f, 0f);
    }

    // 카메라 위치 갱신 - SphereCast로 벽 감지 시 거리 줄임
    private void LateUpdate()
    {
        if (!photonView.IsMine || cameraTransform == null) return;

        Vector3 origin = transform.position;
        Vector3 dir = -transform.forward;
        float targetDistance = cameraDistance;

        if (Physics.SphereCast(origin, collisionRadius, dir, out RaycastHit hit, cameraDistance, collisionMask))
        {
            targetDistance = Mathf.Clamp(hit.distance - collisionOffset, minCameraDistance, cameraDistance);
        }

        currentCameraDistance = Mathf.Lerp(currentCameraDistance, targetDistance, distanceSmoothSpeed * Time.deltaTime);
        cameraTransform.position = origin + dir * currentCameraDistance;
        cameraTransform.rotation = transform.rotation;
    }
}