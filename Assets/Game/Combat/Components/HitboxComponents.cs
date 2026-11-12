using Unity.Entities;

public struct HitboxData : IComponentData
{
    public Entity Owner;
    public float Damage;
    public DamageType Type;
}

public struct HitEntity : IBufferElementData
{
    public Entity Value;
}