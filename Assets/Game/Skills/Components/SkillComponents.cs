using Unity.Entities;
using Unity.Mathematics;

public struct SkillCaster : IComponentData
{
    public Entity SkillPrefab;
    public float Cooldown;
    public float CooldownRemaining;
}

public struct SkillCastRequest : IBufferElementData
{
    public float3 Direction;
}

public struct SkillProjectileData : IComponentData
{
    public float3 SpawnOffset;
    public float Speed;
    public float Lifetime;
}

public struct SkillLifetime : IComponentData
{
    public float Remaining;
}