using Unity.Entities;
using UnityEngine;

[System.Serializable]
public struct ItemModifierEntry
{
    public CharacterStatType Stat;
    public StatModifierType Type;
    public float Value;
}

public class ItemAuthoring : MonoBehaviour
{
    [Header("Item")]
    public int Id;
    public ItemType Type;
    public int MaxStack = 1;

    [Header("Equipment")]
    public EquipmentSlot Slot = EquipmentSlot.None;

    [Header("Stat Modifiers")]
    public ItemModifierEntry[] Modifiers;

    public class Baker : Baker<ItemAuthoring>
    {
        public override void Bake(ItemAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new ItemDefinition
            {
                Id = authoring.Id,
                Type = authoring.Type,
                MaxStack = Mathf.Max(1, authoring.MaxStack),
                Slot = authoring.Slot
            });

            DynamicBuffer<ItemStatModifier> modifiers =
                AddBuffer<ItemStatModifier>(entity);

            foreach (ItemModifierEntry entry in authoring.Modifiers)
            {
                modifiers.Add(new ItemStatModifier
                {
                    Stat = entry.Stat,
                    Type = entry.Type,
                    Value = entry.Value
                });
            }
        }
    }
}