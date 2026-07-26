using NUnit.Framework.Interfaces;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] public GameObject target;
    [SerializeField] private float baseFollowSpeed = 10.0f;
    [SerializeField] private float returnToBaseFollowSpeed = 2.0f;
    [SerializeField] private float defaultCameraShakeSpeed = 0.02f;
    [SerializeField] private float zoomOutSpeed = 2f;

    public bool DisableLerping { get; set; }
    
    private float cameraScale;
    private float followSpeed;
    private float shakeAmount;
    private float shakeResetTime;

    private Camera camera;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        camera = GetComponent<Camera>();
        if (camera == null)
        {
            Debug.LogError("What the hell no camera??");
        }
        
        cameraScale = camera.orthographicSize;
        camera.orthographicSize = 0.5f;
        
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!target)
            return;
        // Return follow speed to default
        followSpeed = Mathf.Lerp(followSpeed, baseFollowSpeed, Time.deltaTime * returnToBaseFollowSpeed);
        shakeAmount = Mathf.Lerp(shakeAmount, 0, Time.deltaTime * shakeResetTime);
        
        // Move to target
        Vector3 targetPosition = Vector3.Lerp(transform.position, target.transform.position, followSpeed * Time.deltaTime);
        
        targetPosition += Random.insideUnitSphere * shakeAmount;
        // Set the position without changing the Z
        transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);

        if (!DisableLerping)
        {
            camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, cameraScale, Time.deltaTime * zoomOutSpeed);
        }
    }

    public void FreezeCameraTemporarily()
    {
        // I love me some hard coded numbers
        followSpeed = 0.0f;
    }

    public void StartCameraShake(float shakeAmount = 0.02f, float shakeResetTime = 0.5f)
    {
        this.shakeAmount = shakeAmount;
        this.shakeResetTime = shakeResetTime;
    }
}
