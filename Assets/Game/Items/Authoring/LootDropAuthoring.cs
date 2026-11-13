using Unity.Entities;
using UnityEngine;

[System.Serializable]
public struct LootDropEntryAuthoring
{
    public GameObject ItemPrefab;

    [Range(0f, 1f)]
    public float DropChance;

    [Min(1)]
    public int MinQuantity;

    [Min(1)]
    public int MaxQuantity;
}

public class LootDropAuthoring : MonoBehaviour
{
    public GameObject PickupPrefab;

    public LootDropEntryAuthoring[] LootTable;

    public class Baker : Baker<LootDropAuthoring>
    {
        public override void Bake(LootDropAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            Entity pickupPrefab = authoring.PickupPrefab != null
                ? GetEntity(
                    authoring.PickupPrefab,
                    TransformUsageFlags.Dynamic)
                : Entity.Null;

            AddComponent(entity, new LootDropSettings
            {
                PickupPrefab = pickupPrefab
            });

            DynamicBuffer<LootDropEntry> lootTable =
                AddBuffer<LootDropEntry>(entity);

            foreach (LootDropEntryAuthoring entry in authoring.LootTable)
            {
                if (entry.ItemPrefab == null)
                    continue;

                lootTable.Add(new LootDropEntry
                {
                    ItemEntity = GetEntity(
                        entry.ItemPrefab,
                        TransformUsageFlags.Dynamic),

                    DropChance = Mathf.Clamp01(entry.DropChance),
                    MinQuantity = Mathf.Max(1, entry.MinQuantity),
                    MaxQuantity = Mathf.Max(
                        Mathf.Max(1, entry.MinQuantity),
                        entry.MaxQuantity)
                });
            }
        }
    }
}