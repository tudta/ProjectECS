using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct InventoryAddSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityManager entityManager = state.EntityManager;

        foreach (var (capacity, inventoryBuffer, requestBuffer) in
            SystemAPI.Query<
                RefRO<InventoryCapacity>,
                DynamicBuffer<InventoryItem>,
                DynamicBuffer<InventoryAddRequest>>())
        {
            DynamicBuffer<InventoryItem> inventory = inventoryBuffer;
            DynamicBuffer<InventoryAddRequest> requests = requestBuffer;
            for (int r = 0; r < requests.Length; r++)
            {
                InventoryAddRequest request = requests[r];

                if (request.Quantity <= 0 ||
                    request.ItemEntity == Entity.Null ||
                    !entityManager.Exists(request.ItemEntity) ||
                    !entityManager.HasComponent<ItemDefinition>(
                        request.ItemEntity))
                    continue;

                ItemDefinition definition =
                    entityManager.GetComponentData<ItemDefinition>(
                        request.ItemEntity);

                int remaining = request.Quantity;
                int maxStack = math.max(1, definition.MaxStack);

                // Fill existing stacks first.
                if (maxStack > 1)
                {
                    for (int i = 0; i < inventory.Length && remaining > 0; i++)
                    {
                        InventoryItem existing = inventory[i];

                        if (existing.ItemEntity != request.ItemEntity)
                            continue;

                        int space = maxStack - existing.Quantity;
                        int added = math.min(space, remaining);

                        if (added <= 0)
                            continue;

                        existing.Quantity += added;
                        inventory[i] = existing;
                        remaining -= added;
                    }
                }

                // Create new stacks while there is inventory space.
                while (remaining > 0 &&
                       inventory.Length < capacity.ValueRO.Value)
                {
                    int added = math.min(maxStack, remaining);

                    inventory.Add(new InventoryItem
                    {
                        ItemEntity = request.ItemEntity,
                        Quantity = added
                    });

                    remaining -= added;
                }
            }

            requests.Clear();
        }
    }
}