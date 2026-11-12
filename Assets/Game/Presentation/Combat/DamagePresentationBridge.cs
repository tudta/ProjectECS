using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class DamagePresentationBridge : MonoBehaviour
{
    [Header("Damage Numbers")]
    [SerializeField]
    private FloatingDamageNumber damageNumberPrefab;

    [SerializeField]
    private Vector3 damageNumberOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Effects")]
    [SerializeField]
    private HitEffectLibrary effectLibrary;

    private World world;
    private EntityQuery eventQuery;
    private bool queryCreated;

    private void LateUpdate()
    {
        World currentWorld =
            World.DefaultGameObjectInjectionWorld;

        if (currentWorld == null || !currentWorld.IsCreated)
            return;

        // Recreate query if the ECS World changes.
        if (world != currentWorld)
        {
            DisposeQuery();

            world = currentWorld;

            eventQuery = world.EntityManager.CreateEntityQuery(
                ComponentType.ReadWrite<DamageReceivedEvent>(),
                ComponentType.ReadWrite<DeathEvent>()
            );

            queryCreated = true;
        }

        if (!queryCreated ||
            eventQuery.CalculateEntityCount() != 1)
            return;

        EntityManager manager = world.EntityManager;

        Entity eventEntity = eventQuery.GetSingletonEntity();

        ProcessDamageEvents(manager, eventEntity);
        ProcessDeathEvents(manager, eventEntity);
    }

    private void ProcessDamageEvents(
        EntityManager manager,
        Entity eventEntity)
    {
        DynamicBuffer<DamageReceivedEvent> events =
            manager.GetBuffer<DamageReceivedEvent>(eventEntity);

        for (int i = 0; i < events.Length; i++)
        {
            DamageReceivedEvent damageEvent = events[i];

            if (!manager.Exists(damageEvent.Target))
                continue;

            Vector3 position = GetEntityPosition(
                manager,
                damageEvent.Target
            );

            // Floating damage number.
            if (damageNumberPrefab != null)
            {
                FloatingDamageNumber number = Instantiate(
                    damageNumberPrefab,
                    position + damageNumberOffset,
                    Quaternion.identity
                );

                number.Initialize(damageEvent.Amount);
            }

            // Hit visual effect.
            if (effectLibrary != null)
            {
                GameObject prefab =
                    effectLibrary.GetHitPrefab(damageEvent.Type);

                if (prefab != null)
                {
                    Destroy(
                        Instantiate(prefab, position, Quaternion.identity),
                        5f
                    );
                }

                // Hit audio.
                AudioClip sound =
                    effectLibrary.GetHitSound(damageEvent.Type);

                if (sound != null)
                {
                    AudioSource.PlayClipAtPoint(sound, position);
                }
            }
        }

        // Consume processed events.
        events.Clear();
    }

    private void ProcessDeathEvents(
        EntityManager manager,
        Entity eventEntity)
    {
        DynamicBuffer<DeathEvent> events =
            manager.GetBuffer<DeathEvent>(eventEntity);

        for (int i = 0; i < events.Length; i++)
        {
            DeathEvent deathEvent = events[i];

            if (!manager.Exists(deathEvent.Entity))
                continue;

            Vector3 position = GetEntityPosition(
                manager,
                deathEvent.Entity
            );

            if (effectLibrary == null)
                continue;

            // Death visual effect.
            if (effectLibrary.deathPrefab != null)
            {
                Destroy(
                    Instantiate(
                        effectLibrary.deathPrefab,
                        position,
                        Quaternion.identity
                    ),
                    5f
                );
            }

            // Death audio.
            if (effectLibrary.deathSound != null)
            {
                AudioSource.PlayClipAtPoint(
                    effectLibrary.deathSound,
                    position
                );
            }
        }

        // Consume processed events.
        events.Clear();
    }

    private Vector3 GetEntityPosition(
        EntityManager manager,
        Entity entity)
    {
        if (!manager.HasComponent<LocalToWorld>(entity))
            return Vector3.zero;

        var transform = manager.GetComponentData<LocalToWorld>(
            entity
        );

        return new Vector3(
            transform.Position.x,
            transform.Position.y,
            transform.Position.z
        );
    }

    private void DisposeQuery()
    {
        if (queryCreated && world != null && world.IsCreated)
        {
            eventQuery.Dispose();
        }

        queryCreated = false;
    }

    private void OnDestroy()
    {
        DisposeQuery();
    }
}