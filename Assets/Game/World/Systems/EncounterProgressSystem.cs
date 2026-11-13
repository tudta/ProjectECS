using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EncounterSpawnSystem))]
[UpdateAfter(typeof(DeathSystem))]
public partial struct EncounterProgressSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityManager em = state.EntityManager;

        // Only living encounter enemies are included.
        EntityQuery aliveEnemiesQuery = SystemAPI.QueryBuilder()
            .WithAll<EncounterMember, CharacterTag>()
            .WithNone<DeadTag>()
            .Build();

        using NativeArray<EncounterMember> aliveMembers =
            aliveEnemiesQuery.ToComponentDataArray<EncounterMember>(
                Allocator.Temp);

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
            if (encounterState.ValueRO.IsActive == 0 ||
                encounterState.ValueRO.IsComplete != 0)
                continue;

            int currentWave = encounterState.ValueRO.CurrentWave;

            // Check whether any enemies from this wave remain alive.
            bool enemiesAlive = false;

            for (int i = 0; i < aliveMembers.Length; i++)
            {
                EncounterMember member = aliveMembers[i];

                if (member.Encounter == encounterEntity &&
                    member.WaveIndex == currentWave)
                {
                    enemiesAlive = true;
                    break;
                }
            }

            if (enemiesAlive)
                continue;

            int nextWave = currentWave + 1;
            bool nextWaveExists = false;

            foreach (EncounterWaveEntry entry in entries)
            {
                if (entry.WaveIndex == nextWave)
                {
                    nextWaveExists = true;
                    break;
                }
            }

            if (!nextWaveExists)
            {
                encounterState.ValueRW.IsComplete = 1;
                continue;
            }

            uint seed = (uint)(
                encounterEntity.Index * 73856093 ^
                encounterEntity.Version * 19349663 ^
                nextWave * 83492791);

            Random random = new Random(math.max(seed, 1u));

            EncounterSpawnUtility.SpawnWave(                encounterEntity,
                nextWave,
                transform.ValueRO.Position,
                settings.ValueRO.SpawnRadius,
                entries,
                ref random,
                ref ecb,
                em);

            encounterState.ValueRW.CurrentWave = nextWave;
        }

        ecb.Playback(em);
        ecb.Dispose();
    }
}