namespace Player
{
    public interface IShoot
    {
        float GetReloadPercentage();
        int GetBullets();
        int GetMaxBullets();
        float GetGunReadyToFirePercentage();
    }
}