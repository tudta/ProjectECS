using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct DamageProcessingSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingletonBuffer<DamageRequest>(
            out var requests))
            return;

        if (!SystemAPI.TryGetSingletonBuffer<DamageReceivedEvent>(
            out var damageEvents))
            return;

        EntityManager entityManager = state.EntityManager;

        for (int i = 0; i < requests.Length; i++)
        {
            DamageRequest request = requests[i];

            // Validate target.
            if (request.Target == Entity.Null ||
                !entityManager.Exists(request.Target))
                continue;

            if (!entityManager.HasComponent<Health>(request.Target))
                continue;

            if (entityManager.HasComponent<DeadTag>(request.Target))
                continue;

            float damage = math.max(0f, request.Amount);

            // Physical damage uses armor.
            // Handle custom physical damage formula here.
            if (request.Type == DamageType.Physical)
            {
                float armor = 0f;

                if (entityManager.HasComponent<Armor>(request.Target))
                {
                    armor = entityManager
                        .GetComponentData<Armor>(request.Target)
                        .Value;
                }

                armor = math.max(0f, armor);

                damage *= 100f / (100f + armor);
            }
            else
            {
                // Handle custom elemental damage formula here.
                float resistance = 0f;

                if (entityManager.HasComponent<DamageResistance>(
                    request.Target))
                {
                    var resistances = entityManager
                        .GetComponentData<DamageResistance>(
                            request.Target);

                    switch (request.Type)
                    {
                        case DamageType.Fire:
                            resistance = resistances.Fire;
                            break;

                        case DamageType.Cold:
                            resistance = resistances.Cold;
                            break;

                        case DamageType.Lightning:
                            resistance = resistances.Lightning;
                            break;

                        case DamageType.Poison:
                            resistance = resistances.Poison;
                            break;
                    }
                }

                // Allow vulnerability down to -100%.
                // Handle custom resistance formula here.
                resistance = math.clamp(resistance, -1f, 0.9f);

                damage *= 1f - resistance;
            }

            // Apply damage.
            Health health = entityManager
                .GetComponentData<Health>(request.Target);

            health.Current = math.max(
                0f,
                health.Current - damage
            );

            entityManager.SetComponentData(request.Target, health);

            // Generate damage event.
            damageEvents.Add(new DamageReceivedEvent
            {
                Source = request.Source,
                Target = request.Target,
                Amount = damage,
                Type = request.Type
            });
        }

        // Requests have now been processed.
        requests.Clear();
    }
}