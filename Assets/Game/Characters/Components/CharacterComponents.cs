using Unity.Entities;

public struct CharacterTag : IComponentData
{
}

public struct PlayerTag : IComponentData
{
}

public struct CharacterFaction : IComponentData
{
    public int Value;
}

public struct Health : IComponentData
{
    public float Current;
    public float Maximum;
}

public struct MovementSpeed : IComponentData
{
    public float Value;
}