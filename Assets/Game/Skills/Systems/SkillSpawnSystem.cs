using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(PhysicsSystemGroup))]
public partial struct SkillSpawnSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        EntityManager em = state.EntityManager;

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (caster, requests, transform, entity) in
            SystemAPI.Query<
                RefRW<SkillCaster>,
                DynamicBuffer<SkillCastRequest>,
                RefRO<LocalTransform>>()
            .WithAll<PlayerTag>()
            .WithEntityAccess())
        {
            // Update cooldown.
            caster.ValueRW.CooldownRemaining = math.max(
                0f,
                caster.ValueRO.CooldownRemaining - deltaTime
            );

            if (requests.Length == 0)
                continue;

            if (caster.ValueRO.CooldownRemaining > 0f)
                continue;

            Entity prefab = caster.ValueRO.SkillPrefab;

            if (prefab == Entity.Null || !em.Exists(prefab))
            {
                requests.Clear();
                continue;
            }

            if (!em.HasComponent<SkillProjectileData>(prefab) ||
                !em.HasComponent<HitboxData>(prefab))
            {
                requests.Clear();
                continue;
            }

            SkillProjectileData data =
                em.GetComponentData<SkillProjectileData>(prefab);

            SkillCastRequest request = requests[0];

            float3 direction = math.normalizesafe(
                request.Direction,
                new float3(0f, 0f, 1f)
            );

            float3 spawnPosition =
                transform.ValueRO.Position +
                math.rotate(transform.ValueRO.Rotation, data.SpawnOffset);

            quaternion rotation =
                quaternion.LookRotationSafe(direction, math.up());

            // Instantiate the projectile.
            Entity projectile = ecb.Instantiate(prefab);

            ecb.SetComponent(
                projectile,
                LocalTransform.FromPositionRotationScale(
                    spawnPosition,
                    rotation,
                    1f
                )
            );

            // Assign the caster as the projectile's owner.
            HitboxData hitbox =
                em.GetComponentData<HitboxData>(prefab);

            hitbox.Owner = entity;

            ecb.SetComponent(projectile, hitbox);

            // Apply initial projectile velocity.
            if (em.HasComponent<PhysicsVelocity>(prefab))
            {
                ecb.SetComponent(projectile, new PhysicsVelocity
                {
                    Linear = direction * data.Speed,
                    Angular = float3.zero
                });
            }

            // Reset projectile lifetime.
            ecb.SetComponent(projectile, new SkillLifetime
            {
                Remaining = data.Lifetime
            });

            // Consume request and start cooldown.
            requests.Clear();

            caster.ValueRW.CooldownRemaining =
                caster.ValueRO.Cooldown;
        }

        ecb.Playback(em);
        ecb.Dispose();
    }
}