using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
    [SerializeField] private Transform position1;
    [SerializeField] private Transform position2;
    [SerializeField] private float distanceThreshold = 0.1f;
    [SerializeField] private float cameraMoveSpeed = 1f;

    private Vector3 targetPosition;
    private bool positionTarget;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector2.Distance(transform.position, targetPosition) <= distanceThreshold)
        {
            if (positionTarget)
            {
                targetPosition = position1.position;
            }
            else
            {
                targetPosition = position2.position;
            }
            
            positionTarget = !positionTarget;
        }
        
        transform.position = Vector3.Lerp(transform.position, new Vector3(targetPosition.x, targetPosition.y, transform.position.z), cameraMoveSpeed * Time.unscaledDeltaTime);
    }
}
