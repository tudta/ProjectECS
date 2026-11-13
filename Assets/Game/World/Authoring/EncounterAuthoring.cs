using Unity.Entities;
using UnityEngine;

public class EncounterAuthoring : MonoBehaviour
{
    [Header("Activation")]
    [Min(0.1f)]
    public float ActivationRadius = 8f;

    [Header("Enemy Spawning")]
    [Min(0f)]
    public float SpawnRadius = 5f;

    public EncounterWaveAuthoring[] Waves;

    [System.Serializable]
    public class EncounterWaveAuthoring
    {
        public EnemySpawnEntryAuthoring[] Enemies;
    }

    [System.Serializable]
    public class EnemySpawnEntryAuthoring
    {
        public GameObject EnemyPrefab;

        [Min(1)]
        public int Count = 1;
    }

    public class Baker : Baker<EncounterAuthoring>
    {
        public override void Bake(EncounterAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new EncounterSettings
            {
                ActivationRadius = Mathf.Max(
                    0.1f, authoring.ActivationRadius),

                SpawnRadius = Mathf.Max(
                    0f, authoring.SpawnRadius)
            });

            AddComponent(entity, new EncounterState
            {
                CurrentWave = -1,
                IsActive = 0,
                IsComplete = 0
            });

            DynamicBuffer<EncounterWaveEntry> entries =
                AddBuffer<EncounterWaveEntry>(entity);

            if (authoring.Waves == null)
                return;

            for (int waveIndex = 0;
                 waveIndex < authoring.Waves.Length;
                 waveIndex++)
            {
                var wave = authoring.Waves[waveIndex];

                if (wave == null || wave.Enemies == null)
                    continue;

                foreach (var enemy in wave.Enemies)
                {
                    if (enemy == null || enemy.EnemyPrefab == null)
                        continue;

                    entries.Add(new EncounterWaveEntry
                    {
                        EnemyPrefab = GetEntity(
                            enemy.EnemyPrefab,
                            TransformUsageFlags.Dynamic),

                        WaveIndex = waveIndex,
                        Count = Mathf.Max(1, enemy.Count)
                    });
                }
            }
        }
    }
}