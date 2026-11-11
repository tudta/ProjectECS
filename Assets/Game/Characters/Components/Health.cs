using Unity.Entities;

public struct Health : IComponentData
{
    public float Current;
    public float Maximum;
}