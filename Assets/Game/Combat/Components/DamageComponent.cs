using Unity.Entities;

public enum DamageType : byte
{
    Physical,
    Fire,
    Cold,
    Lightning,
    Poison
}

public struct DamageRequest : IBufferElementData
{
    public Entity Source;
    public Entity Target;

    public float Amount;
    public DamageType Type;
}

public struct Armor : IComponentData
{
    public float Value;
}

public struct DamageResistance : IComponentData
{
    public float Physical;
    public float Fire;
    public float Cold;
    public float Lightning;
    public float Poison;
}