using UnityEngine;
using UnityEngine.EventSystems;

public class ShopItemAnimation : MonoBehaviour
{
    [SerializeField] private float scaleFactor = 1.05f;
    [SerializeField] private float scaleSpeed = 10f;
    
    private Vector3 startingScale;
    private Vector3 targetScale;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startingScale = transform.localScale;
        targetScale = startingScale;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, scaleSpeed * Time.unscaledDeltaTime);
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
