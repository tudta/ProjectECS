using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemyAISystem))]
public partial struct EnemyAttackSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        EntityManager em = state.EntityManager;

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (attack, aiSettings, target, transform, entity) in
            SystemAPI.Query<
                RefRW<EnemyAttackSettings>,
                RefRO<EnemyAISettings>,
                RefRO<EnemyAITarget>,
                RefRW<LocalTransform>>()
            .WithAll<EnemyAITag>()
            .WithNone<DeadTag>()
            .WithEntityAccess())
        {
            // Update attack cooldown.
            attack.ValueRW.CooldownRemaining = math.max(
                0f,
                attack.ValueRO.CooldownRemaining - deltaTime
            );

            Entity targetEntity = target.ValueRO.Value;

            if (targetEntity == Entity.Null ||
                !em.Exists(targetEntity) ||
                em.HasComponent<DeadTag>(targetEntity))
                continue;

            if (!em.HasComponent<LocalTransform>(targetEntity))
                continue;

            float3 enemyPosition = transform.ValueRO.Position;

            float3 targetPosition =
                em.GetComponentData<LocalTransform>(targetEntity).Position;

            float3 offset = targetPosition - enemyPosition;
            offset.y = 0f;

            float distanceSq = math.lengthsq(offset);

            float attackRange = aiSettings.ValueRO.AttackRange;

            if (distanceSq > attackRange * attackRange)
                continue;

            // Face the target.
            float3 direction = math.normalizesafe(
                offset,
                new float3(0f, 0f, 1f)
            );

            quaternion rotation =
                quaternion.LookRotationSafe(direction, math.up());

            transform.ValueRW.Rotation = rotation;

            // Wait for cooldown.
            if (attack.ValueRO.CooldownRemaining > 0f)
                continue;

            Entity prefab = attack.ValueRO.HitboxPrefab;

            if (prefab == Entity.Null || !em.Exists(prefab))
                continue;

            if (!em.HasComponent<HitboxData>(prefab) ||
                !em.HasComponent<HitEntity>(prefab) ||
                !em.HasComponent<SkillLifetime>(prefab) ||
                !em.HasComponent<PhysicsVelocity>(prefab))
                continue;

            // Spawn the attack in front of the enemy.
            float3 spawnPosition =
                enemyPosition +
                direction * attack.ValueRO.ForwardOffset;

            Entity hitbox = ecb.Instantiate(prefab);

            ecb.SetComponent(
                hitbox,
                LocalTransform.FromPositionRotationScale(
                    spawnPosition,
                    rotation,
                    1f
                )
            );

            // Assign the enemy as the attack owner.
            HitboxData hitboxData =
                em.GetComponentData<HitboxData>(prefab);

            hitboxData.Owner = entity;

            ecb.SetComponent(hitbox, hitboxData);

            // Keep the melee hitbox stationary.
            ecb.SetComponent(hitbox, new PhysicsVelocity
            {
                Linear = float3.zero,
                Angular = float3.zero
            });

            // Start the cooldown.
            attack.ValueRW.CooldownRemaining =
                attack.ValueRO.Cooldown;
        }

        ecb.Playback(em);
        ecb.Dispose();
    }
}