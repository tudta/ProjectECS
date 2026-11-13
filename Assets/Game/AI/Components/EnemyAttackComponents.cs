using Unity.Entities;

public struct EnemyAttackSettings : IComponentData
{
    public Entity HitboxPrefab;

    public float Cooldown;
    public float CooldownRemaining;

    public float ForwardOffset;
}