using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObject;
    public Rigidbody rigidBody;

    [Header("Settings")]
    public float rotationSpeed;
    public bool lockRotation = false;

    private void Start()
    {
        ConfigureCursor();
    }

    private void Update()
    {
        UpdateCameraDirection();
        RotatePlayerObject();
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
        return player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
    }

    private void RotatePlayerObject()
    {
        if (lockRotation) return;

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
}
