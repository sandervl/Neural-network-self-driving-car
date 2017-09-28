using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarMovement : MonoBehaviour
{

    //First is forward/backwards
    //Second is left/right
    public List<float> Inputs;

    public float Acceleration;

    public float MaxSpeed;
    public float Friction;

    public float RotationSpeed;

    public float SpeedInput
    {
        get
        {
            return Inputs.FirstOrDefault();
        }
    }

    public float RotationInput
    {
        get
        {
            return Inputs.ElementAtOrDefault(1);
        }
    }

    private new Rigidbody2D rigidbody2D;

    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        rigidbody2D.AddForce(transform.right * Acceleration * SpeedInput);
        rigidbody2D.drag = Friction;

        if (rigidbody2D.velocity.magnitude > MaxSpeed)
        {
            rigidbody2D.velocity = rigidbody2D.velocity.normalized * MaxSpeed;
        }

        transform.Rotate(Vector3.forward * RotationSpeed * RotationInput);
        rigidbody2D.angularDrag = Friction;
    }

    public void SetInputs(float[] controlInputs)
    {
        if (Inputs.Count != controlInputs.Length)
        {
            Debug.LogError("Invalid inputs");
            return;
        }

        Inputs = controlInputs.ToList();
    }
}
