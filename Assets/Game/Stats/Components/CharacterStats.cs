using Unity.Entities;

// Values authored as the character's starting stats.
public struct CharacterBaseStats : IComponentData
{
    public float MaxHealth;
    public float MovementSpeed;
    public float Armor;
    public float AttackDamage;
}

// Final values after modifiers have been applied.
public struct CharacterDerivedStats : IComponentData
{
    public float MaxHealth;
    public float MovementSpeed;
    public float Armor;
    public float AttackDamage;
}

// Each modifier can add or multiply a stat.
public enum CharacterStatType : byte
{
    MaxHealth,
    MovementSpeed,
    Armor,
    AttackDamage
}

public enum StatModifierType : byte
{
    Flat,
    Percent
}

public struct CharacterStatModifier : IBufferElementData
{
    public CharacterStatType Stat;
    public StatModifierType Type;
    public float Value;

    // Optional source, such as an equipment or buff entity.
    public Entity Source;
}