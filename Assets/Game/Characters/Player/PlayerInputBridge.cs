
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputBridge : MonoBehaviour
{
    [Header("Input")]
    [SerializeField]
    private InputActionReference moveAction;

    [Header("Camera")]
    [SerializeField]
    private Camera viewCamera;

    private World world;
    private EntityQuery playerQuery;

    private void OnEnable()
    {
        moveAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
    }

    private void Update()
    {
        World currentWorld =
            World.DefaultGameObjectInjectionWorld;

        if (currentWorld == null || !currentWorld.IsCreated)
            return;

        if (world != currentWorld)
        {
            world = currentWorld;

            playerQuery = world.EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly<PlayerTag>(),
                ComponentType.ReadWrite<CharacterMoveCommand>()
            );
        }

        if (playerQuery.CalculateEntityCount() != 1)
            return;

        Entity player = playerQuery.GetSingletonEntity();

        EntityManager entityManager = world.EntityManager;

        Vector2 input =
            moveAction.action.ReadValue<Vector2>();

        Vector3 forward = Vector3.ProjectOnPlane(
            viewCamera.transform.forward,
            Vector3.up
        ).normalized;

        Vector3 right = Vector3.ProjectOnPlane(
            viewCamera.transform.right,
            Vector3.up
        ).normalized;

        Vector3 direction =
            forward * input.y +
            right * input.x;

        direction = Vector3.ClampMagnitude(direction, 1f);

        entityManager.SetComponentData(
            player,
            new CharacterMoveCommand
            {
                Direction = new float3(
                    direction.x,
                    0f,
                    direction.z
                )
            }
        );
    }
}