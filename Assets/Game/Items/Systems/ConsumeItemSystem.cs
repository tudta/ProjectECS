using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(InventoryAddSystem))]
public partial struct ConsumeItemSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityManager entityManager = state.EntityManager;

        foreach (var (inventoryBuffer, requestBuffer, health) in
            SystemAPI.Query<
                DynamicBuffer<InventoryItem>,
                DynamicBuffer<ConsumeItemRequest>,
                RefRW<Health>>())
        {
            DynamicBuffer<InventoryItem> inventory = inventoryBuffer;
            DynamicBuffer<ConsumeItemRequest> requests = requestBuffer;

            for (int r = 0; r < requests.Length; r++)
            {
                Entity itemEntity = requests[r].ItemEntity;

                if (itemEntity == Entity.Null ||
                    !entityManager.Exists(itemEntity) ||
                    !entityManager.HasComponent<ItemDefinition>(itemEntity))
                    continue;

                ItemDefinition definition =
                    entityManager.GetComponentData<ItemDefinition>(itemEntity);

                if (definition.Type != ItemType.Consumable)
                    continue;

                // Find the item in the inventory.
                int inventoryIndex = -1;

                for (int i = 0; i < inventory.Length; i++)
                {
                    if (inventory[i].ItemEntity == itemEntity &&
                        inventory[i].Quantity > 0)
                    {
                        inventoryIndex = i;
                        break;
                    }
                }

                if (inventoryIndex < 0)
                    continue;

                float restoreAmount = 0f;

                switch (definition.EffectType)
                {
                    case ConsumableEffectType.RestoreHealthFlat:
                        restoreAmount = definition.EffectValue;
                        break;

                    case ConsumableEffectType.RestoreHealthPercent:
                        restoreAmount =
                            health.ValueRO.Maximum * definition.EffectValue;
                        break;
                }

                restoreAmount = math.max(0f, restoreAmount);

                float oldHealth = health.ValueRO.Current;
                float newHealth = math.min(
                    health.ValueRO.Maximum,
                    oldHealth + restoreAmount);

                // Don't consume an item if it has no effect.
                if (newHealth <= oldHealth)
                    continue;

                health.ValueRW.Current = newHealth;

                // Consume one item.
                InventoryItem stack = inventory[inventoryIndex];
                stack.Quantity--;

                if (stack.Quantity <= 0)
                    inventory.RemoveAt(inventoryIndex);
                else
                    inventory[inventoryIndex] = stack;
            }

            requests.Clear();
        }
    }
}