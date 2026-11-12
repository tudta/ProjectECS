using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct SkillLifetimeSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (lifetime, entity) in
            SystemAPI.Query<RefRW<SkillLifetime>>()
                .WithEntityAccess())
        {
            lifetime.ValueRW.Remaining -= deltaTime;

            if (lifetime.ValueRO.Remaining <= 0f)
            {
                ecb.DestroyEntity(entity);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}