using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [Header("Keybinds")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode attackKey = KeyCode.Mouse0;

    public float Horizontal { get; private set; }
    public float Vertical { get; private set; }
    public float MoveAmount { get; private set; }
    public bool Sprint { get; private set; }
    public bool Jump { get; private set; }
    public bool Attack { get; private set; }

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
        Sprint = Input.GetKey(sprintKey);
    }
}
