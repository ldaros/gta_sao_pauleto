using UnityEngine;

public class MassAdjuster : MonoBehaviour
{
    private Rigidbody rb;
    public float massChangeStep = 1f;
    private float minMass = 1f;
    private float maxMass = 100f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            rb.mass = Mathf.Min(rb.mass + massChangeStep, maxMass);
        }
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            rb.mass = Mathf.Max(rb.mass - massChangeStep, minMass);
        }
    }
}