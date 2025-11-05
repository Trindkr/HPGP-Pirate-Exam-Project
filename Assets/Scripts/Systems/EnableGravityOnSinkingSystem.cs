using Unity.Burst;
using Unity.Entities;
using Unity.Physics;

[BurstCompile]
public partial struct EnableGravityOnSinkingSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        //TODO add logic for what should happen when ship is sinking
    }
}
