using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(DeathSystem))]
public partial struct LootDropSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityManager entityManager = state.EntityManager;

        EntityCommandBuffer ecb =
            new EntityCommandBuffer(Allocator.Temp);

        double elapsedTime = SystemAPI.Time.ElapsedTime;

        foreach (var (settings, lootTable, transform, entity) in
            SystemAPI.Query<
                RefRO<LootDropSettings>,
                DynamicBuffer<LootDropEntry>,
                RefRO<LocalTransform>>()
                .WithAll<DeadTag>()
                .WithNone<LootProcessedTag>()
                .WithEntityAccess())
        {
            Entity pickupPrefab = settings.ValueRO.PickupPrefab;

            // Validate the pickup prefab.
            if (pickupPrefab == Entity.Null ||
                !entityManager.Exists(pickupPrefab) ||
                !entityManager.HasComponent<WorldItemPickup>(pickupPrefab))
            {
                ecb.AddComponent<LootProcessedTag>(entity);
                continue;
            }

            // Create a deterministic but varied random seed.
            uint seed = math.hash(new uint3(
                (uint)entity.Index,
                (uint)entity.Version,
                (uint)(elapsedTime * 1000.0)));

            if (seed == 0)
                seed = 1;

            Random random = new Random(seed);

            for (int i = 0; i < lootTable.Length; i++)
            {
                LootDropEntry entry = lootTable[i];

                if (entry.ItemEntity == Entity.Null ||
                    !entityManager.Exists(entry.ItemEntity) ||
                    !entityManager.HasComponent<ItemDefinition>(
                        entry.ItemEntity))
                    continue;

                if (random.NextFloat() > math.saturate(entry.DropChance))
                    continue;

                int minQuantity = math.max(1, entry.MinQuantity);
                int maxQuantity = math.max(
                    minQuantity, entry.MaxQuantity);

                int quantity = random.NextInt(
                    minQuantity, maxQuantity + 1);

                Entity pickup = ecb.Instantiate(pickupPrefab);

                float3 offset = new float3(
                    random.NextFloat(-0.5f, 0.5f),
                    0.25f,
                    random.NextFloat(-0.5f, 0.5f));

                LocalTransform pickupTransform = transform.ValueRO;
                pickupTransform.Position += offset;

                ecb.SetComponent(pickup, pickupTransform);

                ecb.SetComponent(pickup, new WorldItemPickup
                {
                    ItemEntity = entry.ItemEntity,
                    Quantity = quantity
                });
            }

            // Prevent this enemy from dropping loot again.
            ecb.AddComponent<LootProcessedTag>(entity);
        }

        ecb.Playback(entityManager);
        ecb.Dispose();
    }
}