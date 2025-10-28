using Unity.Entities;

public struct CannonConstraints : IComponentData
{
    public float ShootingForce;
    public float ReloadTime;
    public float ReloadTimer;
    public bool FireLeft;
}
