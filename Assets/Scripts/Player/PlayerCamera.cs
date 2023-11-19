using System;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObject;
    public Rigidbody rigidBody;
    public CinemachineFreeLook cinemachineCamera;
    public Camera mainCamera;
    public RawImage crosshair;
    public Transform aimLookAt;

    [Header("Settings")]
    public float rotationSpeed;
    public bool lockRotation = false;
    public float normalFOV = 40f;
    public float aimingFOV = 20f;

    private PlayerManager playerManager;

    private void Start()
    {
        InitializeComponents();
        ConfigureCursor();
        SetNormalView();
    }

    private void InitializeComponents()
    {
        playerManager = GetComponent<PlayerManager>();
    }

    private void Update()
    {
        if (!lockRotation)
        {
            UpdateCameraDirection();
            if (playerManager.IsAiming && playerManager.IsGrounded)
            {
                RotatePlayerTowardsCameraDirection();
                SetAimView();
            }
            else
            {
                RotatePlayerObject();
                SetNormalView();
            }
        }
    }

    private void SetAimView()
    {
        cinemachineCamera.LookAt = aimLookAt;
        cinemachineCamera.m_Lens.FieldOfView = aimingFOV;
        crosshair.enabled = true;

    }

    private void SetNormalView()
    {
        cinemachineCamera.LookAt = player;
        cinemachineCamera.m_Lens.FieldOfView = normalFOV;
        crosshair.enabled = false;
    }

    private void ConfigureCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UpdateCameraDirection()
    {
        Vector3 viewDirection = CalculateViewDirection();
        orientation.forward = viewDirection.normalized;
    }

    private Vector3 CalculateViewDirection()
    {
        float playerY = player.position.y;
        Vector3 cameraXZPosition = new Vector3(cinemachineCamera.transform.position.x, playerY, cinemachineCamera.transform.position.z);
        return player.position - cameraXZPosition;
    }

    private void RotatePlayerObject()
    {
        Vector3 inputDirection = GetInputDirection();
        if (inputDirection != Vector3.zero)
        {
            playerObject.forward = Vector3.Slerp(playerObject.forward, inputDirection, Time.deltaTime * rotationSpeed);
        }
    }

    private void RotatePlayerTowardsCameraDirection()
    {
        Vector3 cameraDirection = orientation.forward;
        cameraDirection.y = 0; // Keep only the horizontal direction
        playerObject.forward = Vector3.Slerp(playerObject.forward, cameraDirection, Time.deltaTime * rotationSpeed);
    }

    private Vector3 GetInputDirection()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        return orientation.forward * verticalInput + orientation.right * horizontalInput;
    }

    public Camera GetMainCamera()
    {
        return mainCamera;
    }

    internal void SetCameraLock(bool enable)
    {
        lockRotation = enable;
    }
}
