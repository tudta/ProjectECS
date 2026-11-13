using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(CharacterMovementSystem))]
[BurstCompile]
public partial struct EnemyAISystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingletonEntity<PlayerTag>(
                out Entity player) ||
            !SystemAPI.HasComponent<LocalTransform>(player) ||
            SystemAPI.HasComponent<DeadTag>(player))
        {
            foreach (var (target, aiState, movement) in
                SystemAPI.Query<
                    RefRW<EnemyAITarget>,
                    RefRW<EnemyAIState>,
                    RefRW<CharacterMoveCommand>>()
                .WithAll<EnemyAITag>()
                .WithNone<DeadTag>())
            {
                target.ValueRW.Value = Entity.Null;
                aiState.ValueRW.IsAggro = 0;
                movement.ValueRW.Direction = float3.zero;
            }

            return;
        }

        // Stop dead enemies from continuing their previous movement.
        foreach (var movement in
            SystemAPI.Query<RefRW<CharacterMoveCommand>>()
                .WithAll<EnemyAITag, DeadTag>())
        {
            movement.ValueRW.Direction = float3.zero;
        }

        float3 playerPosition =
            SystemAPI.GetComponent<LocalTransform>(player).Position;

        foreach (var (settings, target, aiState, movement, transform) in
            SystemAPI.Query<
                RefRO<EnemyAISettings>,
                RefRW<EnemyAITarget>,
                RefRW<EnemyAIState>,
                RefRW<CharacterMoveCommand>,
                RefRO<LocalTransform>>()
            .WithAll<EnemyAITag>()
            .WithNone<DeadTag>())
        {
            float3 enemyPosition = transform.ValueRO.Position;

            float3 offset = playerPosition - enemyPosition;
            offset.y = 0f;

            float distanceSq = math.lengthsq(offset);

            float aggroRangeSq =
                settings.ValueRO.AggroRange *
                settings.ValueRO.AggroRange;

            float attackRangeSq =
                settings.ValueRO.AttackRange *
                settings.ValueRO.AttackRange;

            // Acquire or retain the player target.
            if (distanceSq <= aggroRangeSq)
            {
                target.ValueRW.Value = player;
                aiState.ValueRW.IsAggro = 1;
            }
            else
            {
                target.ValueRW.Value = Entity.Null;
                aiState.ValueRW.IsAggro = 0;
            }

            if (aiState.ValueRO.IsAggro == 0)
            {
                movement.ValueRW.Direction = float3.zero;
                continue;
            }

            // Stop moving when close enough to attack.
            if (distanceSq <= attackRangeSq)
            {
                movement.ValueRW.Direction = float3.zero;
                continue;
            }

            movement.ValueRW.Direction =
                math.normalizesafe(offset);
        }
    }
}