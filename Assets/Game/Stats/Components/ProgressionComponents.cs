using Unity.Entities;

public struct CharacterExperience : IComponentData
{
    public int Current;
    public int Lifetime;
}

public struct CharacterLevel : IComponentData
{
    public int Value;
    public int BaseExperience;

    public float MaxHealthPerLevel;
    public float MovementSpeedPerLevel;
    public float ArmorPerLevel;
    public float AttackDamagePerLevel;
}

public struct ExperienceGainRequest : IBufferElementData
{
    public int Amount;
    public Entity Source;
}