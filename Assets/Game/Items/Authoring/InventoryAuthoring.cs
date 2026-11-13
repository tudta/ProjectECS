using Unity.Entities;
using UnityEngine;

public class InventoryAuthoring : MonoBehaviour
{
    [Min(1)]
    public int Capacity = 30;
    public GameObject[] ItemDefinitions;

    [Header("Item Pickup")]
    [Min(0.1f)]
    public float PickupRadius = 1.5f;

    public class Baker : Baker<InventoryAuthoring>
    {
        public override void Bake(InventoryAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new InventoryCapacity
            {
                Value = Mathf.Max(1, authoring.Capacity)
            });

            AddComponent(entity, new ItemPickupSettings
            {
                Radius = Mathf.Max(0.1f, authoring.PickupRadius)
            });

            AddBuffer<InventoryItem>(entity);
            AddBuffer<EquippedItem>(entity);
            AddBuffer<InventoryAddRequest>(entity);
            AddBuffer<EquipItemRequest>(entity);
            AddBuffer<UnequipItemRequest>(entity);
            AddBuffer<ConsumeItemRequest>(entity);

            DynamicBuffer<ItemCatalogEntry> catalog =
                AddBuffer<ItemCatalogEntry>(entity);

            foreach (GameObject item in authoring.ItemDefinitions)
            {
                if (item == null)
                    continue;

                catalog.Add(new ItemCatalogEntry
                {
                    ItemEntity = GetEntity(
                        item,
                        TransformUsageFlags.Dynamic)
                });
            }
        }
    }
}