using Unity.Entities;

public struct EncounterSettings : IComponentData
{
    public float ActivationRadius;
    public float SpawnRadius;
}

public struct EncounterState : IComponentData
{
    public int CurrentWave;
    public byte IsActive;
    public byte IsComplete;
}

public struct EncounterWaveEntry : IBufferElementData
{
    public Entity EnemyPrefab;
    public int WaveIndex;
    public int Count;
}

public struct EncounterMember : IComponentData
{
    public Entity Encounter;
    public int WaveIndex;
}