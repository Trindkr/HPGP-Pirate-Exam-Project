using Unity.Entities;

public struct Cannon : IComponentData
{
    public float ShootingForce;
    public float ReloadTime;
    public float ReloadTimer;
    public bool FireLeft;
}
