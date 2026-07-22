using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private GameObject cameraPosition;
    
    [Header("Movement")]
    [SerializeField] private float baseMoveSpeed = 250.0f;
    
    [Header("Dashing")]
    [SerializeField] private float dashVelocity = 20.0f;
    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private float minimumVelocityToDash = 0.05f;
    [SerializeField] private float dashCameraShakeAmount = 0.1f;
    [SerializeField] private float dashCameraShakeResetSpeed = 5.0f;
    
    [Header("Sound Effects")]
    [SerializeField] private AudioSource dashSound;
    
    private Vector2 _input;
    private Rigidbody2D _rb;
    private CameraFollow _cameraFollowScript;
    private float _dashCooldownTime = 0.0f;
    
    
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        if (cameraPosition == null)
        {
            cameraPosition = GameObject.FindGameObjectWithTag("MainCamera");
        }
        
        _cameraFollowScript = cameraPosition.GetComponent<CameraFollow>();
    }

    void Update()
    {
        _dashCooldownTime -= Time.deltaTime;
        _dashCooldownTime = Mathf.Clamp(_dashCooldownTime, 0, dashCooldown);
    }

    void FixedUpdate()
    {
        _rb.AddRelativeForce(_input * baseMoveSpeed, ForceMode2D.Force);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        // Don't do it again when we let go!
        if (context.canceled)
        {
            return;
        }
        
        // Don't do it if we're moving too slow, or the cooldown hasn't finished.
        if (_rb.linearVelocity.magnitude < minimumVelocityToDash || _dashCooldownTime > 0.0f)
        {
            return;
        }
        
        // Freeze Camera for a sec and shake
        _cameraFollowScript.FreezeCameraTemporarily();
        _cameraFollowScript.StartCameraShake(dashCameraShakeAmount, dashCameraShakeResetSpeed);
        
        // Move!
        _rb.AddRelativeForce(_input * dashVelocity, ForceMode2D.Impulse);
        
        // Set the cooldown
        _dashCooldownTime = dashCooldown;
        
        // Sound effect!
        dashSound.Play();
    }
}
