using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(InventoryAddSystem))]
public partial struct ItemPickupSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityManager em = state.EntityManager;

        // This implementation expects exactly one player.
        EntityQuery playerQuery = SystemAPI.QueryBuilder()
            .WithAll<PlayerTag, LocalTransform, ItemPickupSettings,
                InventoryCapacity, InventoryItem>()
            .WithNone<DeadTag>()
            .Build();

        if (playerQuery.CalculateEntityCount() != 1)
        {
            return;
        }

        Entity player = playerQuery.GetSingletonEntity();

        LocalTransform playerTransform =
            em.GetComponentData<LocalTransform>(player);

        float pickupRadius =
            em.GetComponentData<ItemPickupSettings>(player).Radius;

        int capacity =
            em.GetComponentData<InventoryCapacity>(player).Value;

        DynamicBuffer<InventoryItem> inventory =
            em.GetBuffer<InventoryItem>(player);

        EntityCommandBuffer ecb =
            new EntityCommandBuffer(Allocator.Temp);

        foreach (var (pickupData, pickupTransform, pickupEntity) in
            SystemAPI.Query<
                RefRO<WorldItemPickup>,
                RefRO<LocalTransform>>()
                .WithEntityAccess())
        {
            WorldItemPickup pickup = pickupData.ValueRO;

            if (pickup.ItemEntity == Entity.Null ||
                pickup.Quantity <= 0 ||
                !em.Exists(pickup.ItemEntity) ||
                !em.HasComponent<ItemDefinition>(pickup.ItemEntity))
                continue;

            float distanceSq = math.distancesq(
                playerTransform.Position,
                pickupTransform.ValueRO.Position);

            if (distanceSq > pickupRadius * pickupRadius)
                continue;

            ItemDefinition definition =
                em.GetComponentData<ItemDefinition>(pickup.ItemEntity);

            int remaining = pickup.Quantity;
            int maxStack = math.max(1, definition.MaxStack);

            // Fill existing stacks first.
            if (maxStack > 1)
            {
                for (int i = 0; i < inventory.Length && remaining > 0; i++)
                {
                    InventoryItem stack = inventory[i];

                    if (stack.ItemEntity != pickup.ItemEntity)
                        continue;

                    int space = maxStack - stack.Quantity;
                    int added = math.min(space, remaining);

                    if (added <= 0)
                        continue;

                    stack.Quantity += added;
                    inventory[i] = stack;
                    remaining -= added;
                }
            }

            // Create new stacks while there is space.
            while (remaining > 0 && inventory.Length < capacity)
            {
                int added = math.min(maxStack, remaining);

                inventory.Add(new InventoryItem
                {
                    ItemEntity = pickup.ItemEntity,
                    Quantity = added
                });

                remaining -= added;
            }

            // Update or destroy the world pickup.
            if (remaining <= 0)
            {
                ecb.DestroyEntity(pickupEntity);
            }
            else
            {
                pickup.Quantity = remaining;
                em.SetComponentData(pickupEntity, pickup);
            }
        }

        ecb.Playback(em);
        ecb.Dispose();
    }
}