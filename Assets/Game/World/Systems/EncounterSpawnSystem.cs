using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct EncounterSpawnSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityManager em = state.EntityManager;

        if (!SystemAPI.TryGetSingletonEntity<PlayerTag>(
                out Entity player))
            return;

        if (!em.HasComponent<LocalTransform>(player) ||
            em.HasComponent<DeadTag>(player))
            return;

        float3 playerPosition =
            em.GetComponentData<LocalTransform>(player).Position;

        EntityCommandBuffer ecb =
            new EntityCommandBuffer(Allocator.Temp);

        foreach (var (settings, encounterState, entries,
                     transform, encounterEntity) in
            SystemAPI.Query<
                RefRO<EncounterSettings>,
                RefRW<EncounterState>,
                DynamicBuffer<EncounterWaveEntry>,
                RefRO<LocalTransform>>()
                .WithEntityAccess())
        {
            if (encounterState.ValueRO.IsActive != 0 ||
                encounterState.ValueRO.IsComplete != 0)
                continue;

            float distanceSq = math.distancesq(
                playerPosition,
                transform.ValueRO.Position);

            float radius = settings.ValueRO.ActivationRadius;

            if (distanceSq > radius * radius)
                continue;

            uint seed = (uint)(
                encounterEntity.Index * 73856093 ^
                encounterEntity.Version * 19349663);

            Random random = new Random(math.max(seed, 1u));

            EncounterSpawnUtility.SpawnWave(encounterEntity,
                0,
                transform.ValueRO.Position,
                settings.ValueRO.SpawnRadius,
                entries,
                ref random,
                ref ecb,
                em);

            encounterState.ValueRW.CurrentWave = 0;
            encounterState.ValueRW.IsActive = 1;
        }

        ecb.Playback(em);
        ecb.Dispose();
    }
}