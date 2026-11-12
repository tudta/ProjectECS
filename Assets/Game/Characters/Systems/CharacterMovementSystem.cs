
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct CharacterMovementSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (move, speed, transform) in
            SystemAPI.Query<
                RefRO<CharacterMoveCommand>,
                RefRO<MovementSpeed>,
                RefRW<LocalTransform>>()
            .WithAll<CharacterTag>())
        {
            float3 direction = move.ValueRO.Direction;

            // Ignore negligible movement.
            if (math.lengthsq(direction) < 0.0001f)
                continue;

            direction = math.normalizesafe(direction);

            // Move character.
            transform.ValueRW.Position +=
                direction *
                speed.ValueRO.Value *
                deltaTime;

            // Rotate character toward movement direction.
            transform.ValueRW.Rotation =
                quaternion.LookRotationSafe(
                    direction,
                    math.up()
                );
        }
    }
}