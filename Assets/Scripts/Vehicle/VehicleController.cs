using UnityEngine;
using Player;

namespace Vehicle
{
    public class VehicleController : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float height = 1.5f;
        [SerializeField] private PlayerInput input;

        private new Rigidbody rigidbody;

        private bool playerInVehicle = false;
        private bool isGrounded = false;

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            CheckGroundStatus(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (!playerInVehicle || !isGrounded) return;

            rigidbody.AddForce(transform.forward * (input.Vertical * speed * 10f), ForceMode.Acceleration);
            rigidbody.AddTorque(transform.up * (input.Horizontal * rotationSpeed * 10f), ForceMode.Acceleration);
        }

        public void SetPlayerInVehicle(bool value)
        {
            playerInVehicle = value;
        }

        private void CheckGroundStatus(float delta)
        {
            isGrounded =
                Physics.Raycast(transform.position, Vector3.down, height / 2 + 0.2f, groundMask);
        }
    }
}