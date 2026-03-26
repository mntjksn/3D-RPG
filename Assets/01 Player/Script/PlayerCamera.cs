using UnityEngine;

public class PlayerCamera : MonoBehaviour
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
        Cursor.lockState = CursorLockMode.Locked;

        if (player != null)
            actionLock = player.GetComponent<PlayerActionLock>();

        currentXRotation = xRotation;
        currentCameraDistance = cameraDistance;
    }

    private void Update()
    {
        if (actionLock == null || actionLock.CanLook)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, minXRotation, maxXRotation);

            if (player != null)
                player.Rotate(Vector3.up * mouseX);
        }

        currentXRotation = Mathf.Lerp(currentXRotation, xRotation, cameraSmoothSpeed * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(currentXRotation, 0f, 0f);
    }

    private void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        Vector3 origin = transform.position;
        Vector3 dir = -transform.forward;

        float targetDistance = cameraDistance;

        if (Physics.SphereCast(origin, collisionRadius, dir, out RaycastHit hit, cameraDistance, collisionMask))
        {
            targetDistance = hit.distance - collisionOffset;
            targetDistance = Mathf.Clamp(targetDistance, minCameraDistance, cameraDistance);
        }

        currentCameraDistance = Mathf.Lerp(currentCameraDistance, targetDistance, distanceSmoothSpeed * Time.deltaTime);

        cameraTransform.position = origin + dir * currentCameraDistance;
        cameraTransform.rotation = transform.rotation;
    }
}