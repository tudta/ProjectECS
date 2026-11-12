using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
[BurstCompile]
public partial struct HitDetectionSystem : ISystem
{
    private struct HitPair
    {
        public Entity Hitbox;
        public Entity Target;
    }

    [BurstCompile]
    private struct TriggerJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentLookup<HitboxData> Hitboxes;
        [ReadOnly] public ComponentLookup<CharacterTag> Characters;

        public NativeQueue<HitPair>.ParallelWriter Hits;

        public void Execute(TriggerEvent triggerEvent)
        {
            Entity a = triggerEvent.EntityA;
            Entity b = triggerEvent.EntityB;

            if (Hitboxes.HasComponent(a) &&
                Characters.HasComponent(b))
            {
                Hits.Enqueue(new HitPair
                {
                    Hitbox = a,
                    Target = b
                });
            }

            if (Hitboxes.HasComponent(b) &&
                Characters.HasComponent(a))
            {
                Hits.Enqueue(new HitPair
                {
                    Hitbox = b,
                    Target = a
                });
            }
        }
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingletonBuffer<DamageRequest>(
                out var damageRequests))
            return;

        var hits = new NativeQueue<HitPair>(Allocator.TempJob);

        var job = new TriggerJob
        {
            Hitboxes = SystemAPI.GetComponentLookup<HitboxData>(true),
            Characters = SystemAPI.GetComponentLookup<CharacterTag>(true),
            Hits = hits.AsParallelWriter()
        };

        job.Schedule(
            SystemAPI.GetSingleton<SimulationSingleton>(),
            state.Dependency
        ).Complete();

        var entityManager = state.EntityManager;

        while (hits.TryDequeue(out HitPair hit))
        {
            if (!entityManager.Exists(hit.Hitbox) ||
                !entityManager.Exists(hit.Target))
                continue;

            if (!entityManager.HasBuffer<HitEntity>(hit.Hitbox))
                continue;

            HitboxData hitbox =
                entityManager.GetComponentData<HitboxData>(hit.Hitbox);

            if (hitbox.Owner == hit.Target)
                continue;

            if (hitbox.Owner == Entity.Null ||
                !entityManager.Exists(hitbox.Owner))
                continue;

            if (!entityManager.HasComponent<CharacterFaction>(hitbox.Owner) ||
                !entityManager.HasComponent<CharacterFaction>(hit.Target))
                continue;

            int ownerFaction =
                entityManager.GetComponentData<CharacterFaction>(
                    hitbox.Owner).Value;

            int targetFaction =
                entityManager.GetComponentData<CharacterFaction>(
                    hit.Target).Value;

            // Don't damage characters on the same faction.
            if (ownerFaction == targetFaction)
                continue;

            DynamicBuffer<HitEntity> alreadyHit =
                entityManager.GetBuffer<HitEntity>(hit.Hitbox);

            bool duplicate = false;

            for (int i = 0; i < alreadyHit.Length; i++)
            {
                if (alreadyHit[i].Value == hit.Target)
                {
                    duplicate = true;
                    break;
                }
            }

            if (duplicate)
                continue;

            alreadyHit.Add(new HitEntity
            {
                Value = hit.Target
            });

            damageRequests.Add(new DamageRequest
            {
                Source = hitbox.Owner,
                Target = hit.Target,
                Amount = hitbox.Damage,
                Type = hitbox.Type
            });
        }

        hits.Dispose();
    }
}