using System;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObject;
    public Rigidbody rigidBody;

    public Transform cinemachineCamera;

    [Header("Settings")]
    public float rotationSpeed;
    public bool lockRotation = false;

    private void Start()
    {
        ConfigureCursor();
    }

    private void Update()
    {
        if (!lockRotation)
        {
            UpdateCameraDirection();
            RotatePlayerObject();
        }
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
        Vector3 cameraXZPosition = new Vector3(cinemachineCamera.position.x, playerY, cinemachineCamera.position.z);
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

    private Vector3 GetInputDirection()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        return orientation.forward * verticalInput + orientation.right * horizontalInput;
    }

    internal void SetCameraLock(bool enable)
    {
        lockRotation = enable;
    }
}
