using Unity.Entities;
using Unity.Mathematics;

public struct CharacterMoveCommand : IComponentData
{
    public float3 Direction;
}