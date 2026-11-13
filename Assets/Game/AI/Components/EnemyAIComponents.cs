using Unity.Entities;

public struct EnemyAITag : IComponentData
{
    
}

public struct EnemyAISettings : IComponentData
{
    public float AggroRange;
    public float AttackRange;
}

public struct EnemyAITarget : IComponentData
{
    public Entity Value;
}

public struct EnemyAIState : IComponentData
{
    public byte IsAggro;
}