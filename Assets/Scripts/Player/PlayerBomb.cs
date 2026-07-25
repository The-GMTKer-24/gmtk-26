using Player;
using UnityEngine;

public class PlayerBomb : MonoBehaviour, IShoot
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float GetReloadPercentage()
    {
        return 1;
    }

    public int GetBullets()
    {
        return 1;
    }

    public int GetMaxBullets()
    {
        return 1;
    }

    public float GetGunReadyToFirePercentage()
    {
        return 1;
    }
}
