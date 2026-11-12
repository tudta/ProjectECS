using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillInputBridge : MonoBehaviour
{
    [SerializeField]
    private InputActionReference castAction;

    private World world;
    private EntityQuery playerQuery;
    private bool queryCreated;

    private void OnEnable()
    {
        castAction.action.Enable();
    }

    private void OnDisable()
    {
        castAction.action.Disable();
    }

    private void OnDestroy()
    {
        DisposeQuery();
    }

    private void DisposeQuery()
    {
        if (!queryCreated)
            return;

        // The ECS World may already have been destroyed.
        if (world != null && world.IsCreated)
        {
            playerQuery.Dispose();
        }

        queryCreated = false;
    }

    private void Update()
    {
        if (!castAction.action.WasPressedThisFrame())
            return;

        World currentWorld = World.DefaultGameObjectInjectionWorld;

        if (currentWorld == null || !currentWorld.IsCreated)
            return;

        if (world != currentWorld)
        {
            DisposeQuery();

            world = currentWorld;

            playerQuery = world.EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly<PlayerTag>(),
                ComponentType.ReadOnly<SkillCaster>(),
                ComponentType.ReadOnly<LocalTransform>(),
                ComponentType.ReadOnly<CharacterMoveCommand>(),
                ComponentType.ReadWrite<SkillCastRequest>()
            );

            queryCreated = true;
        }

        if (playerQuery.CalculateEntityCount() != 1)
            return;

        EntityManager em = world.EntityManager;
        Entity player = playerQuery.GetSingletonEntity();

        LocalTransform transform =
            em.GetComponentData<LocalTransform>(player);

        CharacterMoveCommand movement =
            em.GetComponentData<CharacterMoveCommand>(player);

        float3 direction = movement.Direction;

        // If stationary, use the player's facing direction.
        if (math.lengthsq(direction) < 0.0001f)
        {
            direction = math.mul(
                transform.Rotation,
                new float3(0f, 0f, 1f)
            );
        }

        direction = math.normalizesafe(
            direction,
            new float3(0f, 0f, 1f)
        );

        DynamicBuffer<SkillCastRequest> requests =
            em.GetBuffer<SkillCastRequest>(player);

        // Keep only one pending cast request.
        if (requests.Length == 0)
        {
            requests.Add(new SkillCastRequest
            {
                Direction = direction
            });
        }
    }
}