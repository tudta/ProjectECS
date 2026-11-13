using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(ExperienceSystem))]
public partial struct StatCalculationSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (baseStats, derivedStats, modifiers,
                     health, speed, armor) in
            SystemAPI.Query<
                RefRO<CharacterBaseStats>,
                RefRW<CharacterDerivedStats>,
                DynamicBuffer<CharacterStatModifier>,
                RefRW<Health>,
                RefRW<MovementSpeed>,
                RefRW<Armor>>())
        {
            CharacterBaseStats baseValue = baseStats.ValueRO;

            float maxHealth = CalculateStat(
                baseValue.MaxHealth,
                CharacterStatType.MaxHealth,
                modifiers);

            float movementSpeed = CalculateStat(
                baseValue.MovementSpeed,
                CharacterStatType.MovementSpeed,
                modifiers);

            float armorValue = CalculateStat(
                baseValue.Armor,
                CharacterStatType.Armor,
                modifiers);

            float attackDamage = CalculateStat(
                baseValue.AttackDamage,
                CharacterStatType.AttackDamage,
                modifiers);

            maxHealth = math.max(1f, maxHealth);
            movementSpeed = math.max(0f, movementSpeed);
            armorValue = math.max(0f, armorValue);
            attackDamage = math.max(0f, attackDamage);

            derivedStats.ValueRW = new CharacterDerivedStats
            {
                MaxHealth = maxHealth,
                MovementSpeed = movementSpeed,
                Armor = armorValue,
                AttackDamage = attackDamage
            };

            health.ValueRW.Maximum = maxHealth;
            health.ValueRW.Current = math.min(
                health.ValueRO.Current, maxHealth);

            speed.ValueRW.Value = movementSpeed;
            armor.ValueRW.Value = armorValue;
        }
    }

    private static float CalculateStat(
        float baseValue,
        CharacterStatType stat,
        DynamicBuffer<CharacterStatModifier> modifiers)
    {
        float flat = 0f;
        float percent = 0f;

        for (int i = 0; i < modifiers.Length; i++)
        {
            CharacterStatModifier modifier = modifiers[i];

            if (modifier.Stat != stat)
                continue;

            if (modifier.Type == StatModifierType.Flat)
                flat += modifier.Value;
            else
                percent += modifier.Value;
        }

        return (baseValue + flat) * (1f + percent);
    }
}