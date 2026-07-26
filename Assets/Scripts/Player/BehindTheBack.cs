using UnityEngine;

public class BehindTheBack : MonoBehaviour
{
    public static BehindTheBack Instance { get; set; }
    
    void Awake()
    {
        BehindTheBack.Instance = this;
    }
}
