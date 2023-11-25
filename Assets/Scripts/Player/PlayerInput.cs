using UnityEngine;

namespace GTASP.Player
{
    public class PlayerInput : MonoBehaviour
    {
        [Header("Keybindings")]
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode attackKey = KeyCode.Q;
        [SerializeField] private KeyCode shootKey = KeyCode.Mouse0;
        [SerializeField] private KeyCode aimKey = KeyCode.Mouse1;
        [SerializeField] private KeyCode enterVehicleKey = KeyCode.F;
        [SerializeField] private KeyCode brakeKey = KeyCode.Space;


        public float Horizontal { get; private set; }
        public float Vertical { get; private set; }
        public float MoveAmount { get; private set; }
        public bool Sprint { get; private set; }
        public bool Jump { get; private set; }
        public bool Attack { get; private set; }
        public bool Aim { get; private set; }
        public bool Shoot { get; private set; }
        public bool EnterVehicle { get; private set; }
        public bool Brake { get; private set; }


        private void Update()
        {
            ProcessMovementInput();
            ProcessActionInput();
        }

        private void ProcessMovementInput()
        {
            Horizontal = Input.GetAxis("Horizontal");
            Vertical = Input.GetAxis("Vertical");
            MoveAmount = Mathf.Clamp01(Mathf.Abs(Horizontal) + Mathf.Abs(Vertical));
        }

        private void ProcessActionInput()
        {
            Jump = Input.GetKeyDown(jumpKey);
            Attack = Input.GetKeyDown(attackKey);
            Shoot = Input.GetKeyDown(shootKey);
            Sprint = Input.GetKey(sprintKey);
            Brake = Input.GetKey(brakeKey);
            Aim = Input.GetKey(aimKey);
            EnterVehicle = Input.GetKeyDown(enterVehicleKey);
        }
    }
}