using AI;
using UnityEngine;
using Player;

namespace Vehicle
{
    public class VehicleController : MonoBehaviour
    {
        [SerializeField] private PlayerInput input;
        [SerializeField] private Rigidbody body;
        [SerializeField] private WheelCollider frontLeftWheel;
        [SerializeField] private WheelCollider frontRightWheel;
        [SerializeField] private WheelCollider backLeftWheel;
        [SerializeField] private WheelCollider backRightWheel;

        [SerializeField] private Transform frontLeftWheelTransform;
        [SerializeField] private Transform frontRightWheelTransform;
        [SerializeField] private Transform backLeftWheelTransform;
        [SerializeField] private Transform backRightWheelTransform;

        [SerializeField] private float acceleration = 100f;
        [SerializeField] private float breakingForce = 100f;
        [SerializeField] private float maxTurnAngle = 15f;

        [SerializeField] private Transform exitPoint;

        [SerializeField] private EngineSound engineSound;

        [SerializeField] private BoxCollider frontCollider;

        private bool playerInVehicle;
        private float currentAcceleration;
        private float currentBreakingForce;
        private float currentTurnAngle;

        public void SetPlayerInVehicle(bool value)
        {
            playerInVehicle = value;

            if (playerInVehicle)
            {
                engineSound.StartEngine(true);
            }
            else
            {
                engineSound.StopEngine();
            }
        }

        public float GetBreakingForce()
        {
            return currentBreakingForce;
        }

        public Vector3 GetExitPoint()
        {
            return exitPoint.position;
        }

        public float GetSpeed()
        {
            return body.velocity.magnitude;
        }

        private void Awake()
        {
            body.centerOfMass = new Vector3(0f, -0.3f, 0f);
        }

        private void FixedUpdate()
        {
            if (!playerInVehicle) return;

            float horizontal = input.Horizontal;
            float vertical = input.Vertical;

            currentAcceleration = vertical * acceleration;

            currentBreakingForce = input.Brake ? breakingForce : 0f;

            frontRightWheel.motorTorque = currentAcceleration;
            frontLeftWheel.motorTorque = currentAcceleration;

            frontRightWheel.brakeTorque = currentBreakingForce;
            frontLeftWheel.brakeTorque = currentBreakingForce;
            backRightWheel.brakeTorque = currentBreakingForce;
            backLeftWheel.brakeTorque = currentBreakingForce;

            currentTurnAngle = horizontal * maxTurnAngle;

            frontRightWheel.steerAngle = currentTurnAngle;
            frontLeftWheel.steerAngle = currentTurnAngle;

            UpdateWheel(frontLeftWheel, frontLeftWheelTransform);
            UpdateWheel(frontRightWheel, frontRightWheelTransform);
            UpdateWheel(backLeftWheel, backLeftWheelTransform);
            UpdateWheel(backRightWheel, backRightWheelTransform);

            CollideWithEnemy();
        }

        private void UpdateWheel(WheelCollider wheelCollider, Transform wheelTransform)
        {
            wheelCollider.GetWorldPose(out var position, out var rotation);
            wheelTransform.position = position;
            wheelTransform.rotation = rotation;
        }

        private void CollideWithEnemy()
        {
            var results = new Collider[5];
            var bounds = frontCollider.bounds;
            var size = Physics.OverlapBoxNonAlloc(bounds.center, bounds.extents, results,
                frontCollider.transform.rotation, LayerMask.GetMask("Enemy"));

            for (int i = 0; i < size; i++)
            {
                EnemyController enemyController = results[i].GetComponent<EnemyController>();

                if (enemyController)
                {
                    enemyController.TakeShot();
                }
            }
        }
    }
}