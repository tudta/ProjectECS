using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct ExperienceSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (experience, level, requests, baseStats) in
            SystemAPI.Query<
                RefRW<CharacterExperience>,
                RefRW<CharacterLevel>,
                DynamicBuffer<ExperienceGainRequest>,
                RefRW<CharacterBaseStats>>())
        {
            if (requests.Length == 0)
                continue;

            for (int i = 0; i < requests.Length; i++)
            {
                int amount = math.max(0, requests[i].Amount);

                experience.ValueRW.Current += amount;
                experience.ValueRW.Lifetime += amount;
            }

            requests.Clear();

            while (experience.ValueRO.Current >=
                   GetRequiredExperience(level.ValueRO))
            {
                experience.ValueRW.Current -=
                    GetRequiredExperience(level.ValueRO);

                level.ValueRW.Value++;

                baseStats.ValueRW.MaxHealth +=
                    level.ValueRO.MaxHealthPerLevel;

                baseStats.ValueRW.MovementSpeed +=
                    level.ValueRO.MovementSpeedPerLevel;

                baseStats.ValueRW.Armor +=
                    level.ValueRO.ArmorPerLevel;

                baseStats.ValueRW.AttackDamage +=
                    level.ValueRO.AttackDamagePerLevel;
            }
        }
    }

    private static int GetRequiredExperience(CharacterLevel level)
    {
        return math.max(1, level.BaseExperience * level.Value);
    }
}