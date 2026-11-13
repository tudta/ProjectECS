using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct EquipmentSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityManager entityManager = state.EntityManager;

        foreach (var (inventoryBuffer, equippedBuffer,
                     requestBuffer, statModifierBuffer,
                     capacity) in
            SystemAPI.Query<
                DynamicBuffer<InventoryItem>,
                DynamicBuffer<EquippedItem>,
                DynamicBuffer<EquipItemRequest>,
                DynamicBuffer<CharacterStatModifier>,
                RefRO<InventoryCapacity>>())
        {
            // Local writable buffer handles.
            DynamicBuffer<InventoryItem> inventory = inventoryBuffer;
            DynamicBuffer<EquippedItem> equipped = equippedBuffer;
            DynamicBuffer<EquipItemRequest> requests = requestBuffer;
            DynamicBuffer<CharacterStatModifier> statModifiers =
                statModifierBuffer;

            for (int r = 0; r < requests.Length; r++)
            {
                Entity itemEntity = requests[r].ItemEntity;

                if (itemEntity == Entity.Null ||
                    !entityManager.Exists(itemEntity) ||
                    !entityManager.HasComponent<ItemDefinition>(itemEntity))
                    continue;

                ItemDefinition definition =
                    entityManager.GetComponentData<ItemDefinition>(itemEntity);

                if (definition.Type != ItemType.Equipment ||
                    definition.Slot == EquipmentSlot.None)
                    continue;

                // Find the requested item in the inventory.
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

                // Find the currently equipped item in this slot.
                int equippedIndex = -1;

                for (int i = 0; i < equipped.Length; i++)
                {
                    if (equipped[i].Slot == definition.Slot)
                    {
                        equippedIndex = i;
                        break;
                    }
                }

                Entity previousItem = equippedIndex >= 0
                    ? equipped[equippedIndex].ItemEntity
                    : Entity.Null;

                // Already equipped.
                if (previousItem == itemEntity)
                    continue;

                // Check whether the previous item can fit back into inventory.
                bool freesInventorySlot =
                    inventory[inventoryIndex].Quantity == 1;

                if (previousItem != Entity.Null &&
                    inventory.Length -
                    (freesInventorySlot ? 1 : 0) >= capacity.ValueRO.Value)
                {
                    continue;
                }

                // Remove one copy of the requested item.
                InventoryItem inventoryItem = inventory[inventoryIndex];
                inventoryItem.Quantity--;

                if (inventoryItem.Quantity <= 0)
                    inventory.RemoveAt(inventoryIndex);
                else
                    inventory[inventoryIndex] = inventoryItem;

                // Remove previous item's stat modifiers.
                if (previousItem != Entity.Null)
                {
                    for (int i = statModifiers.Length - 1; i >= 0; i--)
                    {
                        if (statModifiers[i].Source == previousItem)
                            statModifiers.RemoveAt(i);
                    }

                    // Return previous item to inventory.
                    inventory.Add(new InventoryItem
                    {
                        ItemEntity = previousItem,
                        Quantity = 1
                    });
                }

                // Apply new item's stat modifiers.
                if (entityManager.HasBuffer<ItemStatModifier>(itemEntity))
                {
                    DynamicBuffer<ItemStatModifier> itemModifiers =
                        entityManager.GetBuffer<ItemStatModifier>(
                            itemEntity, true);

                    for (int i = 0; i < itemModifiers.Length; i++)
                    {
                        ItemStatModifier modifier = itemModifiers[i];

                        statModifiers.Add(new CharacterStatModifier
                        {
                            Stat = modifier.Stat,
                            Type = modifier.Type,
                            Value = modifier.Value,
                            Source = itemEntity
                        });
                    }
                }

                // Update equipment slot.
                if (equippedIndex >= 0)
                {
                    equipped[equippedIndex] = new EquippedItem
                    {
                        Slot = definition.Slot,
                        ItemEntity = itemEntity
                    };
                }
                else
                {
                    equipped.Add(new EquippedItem
                    {
                        Slot = definition.Slot,
                        ItemEntity = itemEntity
                    });
                }
            }

            requests.Clear();
        }
    }
}