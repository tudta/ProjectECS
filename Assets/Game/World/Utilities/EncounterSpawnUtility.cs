using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public static class EncounterSpawnUtility
{
    public static void SpawnWave(
        Entity encounter,
        int waveIndex,
        float3 center,
        float spawnRadius,
        DynamicBuffer<EncounterWaveEntry> entries,
        ref Random random,
        ref EntityCommandBuffer ecb,
        EntityManager em)
    {
        foreach (EncounterWaveEntry entry in entries)
        {
            if (entry.WaveIndex != waveIndex ||
                entry.EnemyPrefab == Entity.Null ||
                !em.Exists(entry.EnemyPrefab) ||
                !em.HasComponent<LocalTransform>(entry.EnemyPrefab))
                continue;

            for (int i = 0; i < entry.Count; i++)
            {
                float angle = random.NextFloat(0f, math.PI * 2f);
                float radius = math.sqrt(random.NextFloat()) * spawnRadius;

                float3 offset = new float3(
                    math.cos(angle) * radius,
                    0f,
                    math.sin(angle) * radius);

                Entity enemy = ecb.Instantiate(entry.EnemyPrefab);

                LocalTransform enemyTransform =
                    em.GetComponentData<LocalTransform>(entry.EnemyPrefab);

                enemyTransform.Position = center + offset;

                ecb.SetComponent(enemy, enemyTransform);

                ecb.AddComponent(enemy, new EncounterMember
                {
                    Encounter = encounter,
                    WaveIndex = waveIndex
                });
            }
        }
    }
}