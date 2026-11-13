using Unity.Entities;

public struct LootDropSettings : IComponentData
{
    public Entity PickupPrefab;
}

public struct LootDropEntry : IBufferElementData
{
    public Entity ItemEntity;
    public float DropChance;
    public int MinQuantity;
    public int MaxQuantity;
}

// Added after loot has been processed.
public struct LootProcessedTag : IComponentData
{
    
}

// Data stored on a spawned world pickup.
public struct WorldItemPickup : IComponentData
{
    public Entity ItemEntity;
    public int Quantity;
}

public struct ItemPickupSettings : IComponentData
{
    public float Radius;
}