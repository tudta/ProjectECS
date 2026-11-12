using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

public class DamageTestBridge : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current == null ||
            !Keyboard.current.spaceKey.wasPressedThisFrame)
            return;

        World world = World.DefaultGameObjectInjectionWorld;

        if (world == null || !world.IsCreated)
            return;

        EntityManager manager = world.EntityManager;

        EntityQuery targetQuery = manager.CreateEntityQuery(
            ComponentType.ReadOnly<CharacterTag>(),
            ComponentType.ReadOnly<CharacterFaction>(),
            ComponentType.ReadOnly<Health>(),
            ComponentType.Exclude<PlayerTag>()
        );

        EntityQuery eventQuery = manager.CreateEntityQuery(
            ComponentType.ReadWrite<DamageRequest>(),
            ComponentType.ReadOnly<DamageReceivedEvent>(),
            ComponentType.ReadOnly<DeathEvent>()
        );

        if (targetQuery.IsEmpty || eventQuery.IsEmpty)
        {
            Debug.LogWarning("Enemy or CombatEventManager not found.");

            targetQuery.Dispose();
            eventQuery.Dispose();
            return;
        }

        Entity target = Entity.Null;

        using (NativeArray<Entity> entities =
            targetQuery.ToEntityArray(Allocator.Temp))
        {
            foreach (Entity entity in entities)
            {
                var faction = manager.GetComponentData<CharacterFaction>(
                    entity);

                if (faction.Value == 1)
                {
                    target = entity;
                    break;
                }
            }
        }

        if (target != Entity.Null)
        {
            Entity managerEntity = eventQuery.GetSingletonEntity();

            DynamicBuffer<DamageRequest> requests =
                manager.GetBuffer<DamageRequest>(managerEntity);

            requests.Add(new DamageRequest
            {
                Source = Entity.Null,
                Target = target,
                Amount = 25f,
                Type = DamageType.Physical
            });

            Debug.Log("Submitted 25 physical damage!");
        }
        else
        {
            Debug.LogWarning("No enemy with faction 1 found.");
        }

        targetQuery.Dispose();
        eventQuery.Dispose();
    }
}