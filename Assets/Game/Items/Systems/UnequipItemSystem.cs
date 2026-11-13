using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EquipmentSystem))]
public partial struct UnequipItemSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (inventoryBuffer, equippedBuffer,
                     requestBuffer, statModifierBuffer, capacity) in
            SystemAPI.Query<
                DynamicBuffer<InventoryItem>,
                DynamicBuffer<EquippedItem>,
                DynamicBuffer<UnequipItemRequest>,
                DynamicBuffer<CharacterStatModifier>,
                RefRO<InventoryCapacity>>())
        {
            // Writable local buffer handles.
            DynamicBuffer<InventoryItem> inventory = inventoryBuffer;
            DynamicBuffer<EquippedItem> equipped = equippedBuffer;
            DynamicBuffer<UnequipItemRequest> requests = requestBuffer;
            DynamicBuffer<CharacterStatModifier> statModifiers =
                statModifierBuffer;

            for (int r = 0; r < requests.Length; r++)
            {
                EquipmentSlot requestedSlot = requests[r].Slot;

                int equippedIndex = -1;

                for (int i = 0; i < equipped.Length; i++)
                {
                    if (equipped[i].Slot == requestedSlot)
                    {
                        equippedIndex = i;
                        break;
                    }
                }

                if (equippedIndex < 0)
                    continue;

                // Do not unequip if the inventory has no free slot.
                if (inventory.Length >= capacity.ValueRO.Value)
                    continue;

                Entity itemEntity = equipped[equippedIndex].ItemEntity;

                // Remove modifiers supplied by this item.
                for (int i = statModifiers.Length - 1; i >= 0; i--)
                {
                    if (statModifiers[i].Source == itemEntity)
                        statModifiers.RemoveAt(i);
                }

                // Return the item to inventory.
                inventory.Add(new InventoryItem
                {
                    ItemEntity = itemEntity,
                    Quantity = 1
                });

                // Remove it from the equipment slot.
                equipped.RemoveAt(equippedIndex);
            }

            requests.Clear();
        }
    }
}