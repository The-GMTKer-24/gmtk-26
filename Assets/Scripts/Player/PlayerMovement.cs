using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float baseMoveSpeed = 250.0f;
    [SerializeField] private float dashVelocity = 20.0f;
    [SerializeField] private float minimumVelocityToDash = 0.05f;
    
    private Vector2 _input;
    private Rigidbody2D _rb;
    
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        _rb.AddForce(_input * baseMoveSpeed, ForceMode2D.Force);
        
        Debug.Log(_rb.linearVelocity.magnitude);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
    }

    public void OnDash()
    {
        if (_rb.linearVelocity.magnitude < minimumVelocityToDash)
        {
            return;
        }
        
        _rb.AddForce(_input * dashVelocity, ForceMode2D.Impulse);
    }
}
