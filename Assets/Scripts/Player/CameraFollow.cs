using NUnit.Framework.Interfaces;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private float baseFollowSpeed = 10.0f;
    [SerializeField] private float returnToBaseFollowSpeed = 2.0f;
    [SerializeField] private float defaultCameraShakeSpeed = 0.02f;
    
    private float _followSpeed;
    private float _shakeAmount;
    private float _shakeResetTime;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
        _followSpeed = Mathf.Lerp(_followSpeed, baseFollowSpeed, Time.deltaTime * returnToBaseFollowSpeed);
        _shakeAmount = Mathf.Lerp(_shakeAmount, 0, Time.deltaTime * _shakeResetTime);
        
        // Move to target
        Vector3 targetPosition = Vector3.Lerp(transform.position, target.transform.position, _followSpeed * Time.deltaTime);
        
        targetPosition += Random.insideUnitSphere * _shakeAmount;
        // Set the position without changing the Z
        transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
    }

    public void FreezeCameraTemporarily()
    {
        // I love me some hard coded numbers
        _followSpeed = 0.0f;
    }

    public void StartCameraShake(float shakeAmount = 0.02f, float shakeResetTime = 0.5f)
    {
        _shakeAmount = shakeAmount;
        _shakeResetTime = shakeResetTime;
    }
}
