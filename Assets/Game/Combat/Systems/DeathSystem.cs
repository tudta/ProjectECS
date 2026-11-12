using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(DamageProcessingSystem))]
[BurstCompile]
public partial struct DeathSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingletonBuffer<DeathEvent>(
            out var deathEvents))
            return;

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (health, entity) in
            SystemAPI.Query<RefRO<Health>>()
                .WithAll<CharacterTag>()
                .WithNone<DeadTag>()
                .WithEntityAccess())
        {
            if (health.ValueRO.Current > 0f)
                continue;

            ecb.AddComponent<DeadTag>(entity);

            deathEvents.Add(new DeathEvent
            {
                Entity = entity,
                Killer = Entity.Null
            });
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}