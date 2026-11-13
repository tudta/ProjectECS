using Unity.Entities;

public enum ItemType : byte
{
    Consumable,
    Equipment,
    Material,
    Quest
}

public enum EquipmentSlot : byte
{
    None,
    Weapon,
    Helmet,
    Chest,
    Gloves,
    Boots,
    Ring,
    Amulet
}

// Data baked onto an item prefab entity.
public struct ItemDefinition : IComponentData
{
    public int Id;
    public ItemType Type;
    public int MaxStack;
    public EquipmentSlot Slot;

    public ConsumableEffectType EffectType;
    public float EffectValue;
}

// Stat bonuses provided by an item.
public struct ItemStatModifier : IBufferElementData
{
    public CharacterStatType Stat;
    public StatModifierType Type;
    public float Value;
}

// An item stored in a character's inventory.
public struct InventoryItem : IBufferElementData
{
    public Entity ItemEntity;
    public int Quantity;
}

// An equipment slot and its currently equipped item.
public struct EquippedItem : IBufferElementData
{
    public EquipmentSlot Slot;
    public Entity ItemEntity;
}

public struct InventoryCapacity : IComponentData
{
    public int Value;
}

public struct InventoryAddRequest : IBufferElementData
{
    public Entity ItemEntity;
    public int Quantity;
}

public struct ItemCatalogEntry : IBufferElementData
{
    public Entity ItemEntity;
}

public struct EquipItemRequest : IBufferElementData
{
    public Entity ItemEntity;
}

public struct UnequipItemRequest : IBufferElementData
{
    public EquipmentSlot Slot;
}

public enum ConsumableEffectType : byte
{
    None,
    RestoreHealthFlat,
    RestoreHealthPercent
}

public struct ConsumeItemRequest : IBufferElementData
{
    public Entity ItemEntity;
}