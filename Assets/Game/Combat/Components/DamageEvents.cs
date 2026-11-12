using Unity.Entities;

public struct DamageReceivedEvent : IBufferElementData
{
    public Entity Source;
    public Entity Target;

    public float Amount;
    public DamageType Type;
}

public struct DeathEvent : IBufferElementData
{
    public Entity Entity;
    public Entity Killer;
}