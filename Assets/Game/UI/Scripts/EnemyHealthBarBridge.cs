using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class EnemyHealthBarBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyHealthBarView healthBarPrefab;
    [SerializeField] private Camera viewCamera;

    [Header("Display")]
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float verticalOffset = 2f;

    private World world;
    private EntityQuery enemyQuery;
    private bool queryCreated;

    private readonly Dictionary<Entity, EnemyHealthBarView> healthBars =
        new Dictionary<Entity, EnemyHealthBarView>();

    private readonly Dictionary<Entity, float> previousHealth =
        new Dictionary<Entity, float>();

    private void LateUpdate()
    {
        World currentWorld = World.DefaultGameObjectInjectionWorld;

        if (currentWorld == null || !currentWorld.IsCreated)
            return;

        if (world != currentWorld)
        {
            DisposeQuery();
            ClearHealthBars();

            world = currentWorld;

            enemyQuery = world.EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly<CharacterTag>(),
                ComponentType.ReadOnly<Health>(),
                ComponentType.ReadOnly<LocalToWorld>(),
                ComponentType.Exclude<PlayerTag>()
            );

            queryCreated = true;
        }

        EntityManager em = world.EntityManager;

        using NativeArray<Entity> enemies =
            enemyQuery.ToEntityArray(Allocator.Temp);

        foreach (Entity enemy in enemies)
        {
            Health health = em.GetComponentData<Health>(enemy);

            LocalToWorld transform =
                em.GetComponentData<LocalToWorld>(enemy);

            if (!previousHealth.TryGetValue(enemy, out float lastHealth))
            {
                previousHealth.Add(enemy, health.Current);
            }
            else if (!Mathf.Approximately(lastHealth, health.Current))
            {
                previousHealth[enemy] = health.Current;

                EnemyHealthBarView bar = GetOrCreateBar(enemy);

                if (bar != null)
                {
                    bar.SetHealth(health.Current, health.Maximum);
                    bar.ShowFor(displayDuration);
                }
            }

            if (healthBars.TryGetValue(enemy, out EnemyHealthBarView view))
            {
                if (view != null)
                {
                    Vector3 position =
                        (Vector3)transform.Position + Vector3.up * verticalOffset;

                    view.transform.position = position;

                    if (viewCamera != null)
                    {
                        view.transform.rotation = viewCamera.transform.rotation;
                    }

                    // if (viewCamera != null)
                    // {
                    //     view.transform.rotation =
                    //         Quaternion.LookRotation(
                    //             viewCamera.transform.position - position,
                    //             viewCamera.transform.up
                    //         );
                    // }
                }
            }
        }

        // Remove references to entities that no longer exist.
        List<Entity> removedEntities = null;

        foreach (var pair in healthBars)
        {
            if (em.Exists(pair.Key))
                continue;

            removedEntities ??= new List<Entity>();
            removedEntities.Add(pair.Key);

            if (pair.Value != null)
                Destroy(pair.Value.gameObject);
        }

        if (removedEntities != null)
        {
            foreach (Entity entity in removedEntities)
            {
                healthBars.Remove(entity);
                previousHealth.Remove(entity);
            }
        }
    }

    private EnemyHealthBarView GetOrCreateBar(Entity enemy)
    {
        if (healthBars.TryGetValue(enemy, out EnemyHealthBarView existing))
            return existing;

        if (healthBarPrefab == null)
            return null;

        EnemyHealthBarView instance = Instantiate(healthBarPrefab);

        healthBars.Add(enemy, instance);

        return instance;
    }

    private void OnDestroy()
    {
        DisposeQuery();
        ClearHealthBars();
    }

    private void DisposeQuery()
    {
        if (!queryCreated)
            return;

        if (world != null && world.IsCreated)
            enemyQuery.Dispose();

        queryCreated = false;
    }

    private void ClearHealthBars()
    {
        foreach (var pair in healthBars)
        {
            if (pair.Value != null)
                Destroy(pair.Value.gameObject);
        }

        healthBars.Clear();
        previousHealth.Clear();
    }
}