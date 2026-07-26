using UnityEngine;
using UnityEngine.EventSystems;

public class ShopItemAnimation : MonoBehaviour
{
    [SerializeField] private float scaleFactor = 2f;
    [SerializeField] private float scaleSpeed = 2f;
    
    private Vector3 startingScale;
    private Vector3 targetScale;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startingScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, scaleSpeed * Time.deltaTime);
    }

    public void PointerEnter(BaseEventData eventData)
    {
        targetScale = startingScale * scaleFactor;
    }

    public void PointerExit(BaseEventData eventData)
    {
        targetScale = startingScale;
    }
}
