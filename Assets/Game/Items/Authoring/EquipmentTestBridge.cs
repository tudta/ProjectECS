using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

public class EquipmentTestBridge : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current == null)
            return;

        World world = World.DefaultGameObjectInjectionWorld;

        if (world == null || !world.IsCreated)
            return;

        EntityManager em = world.EntityManager;

        EntityQuery query = em.CreateEntityQuery(
            ComponentType.ReadOnly<PlayerTag>(),
            ComponentType.ReadWrite<InventoryAddRequest>(),
            ComponentType.ReadOnly<ItemCatalogEntry>(),
            ComponentType.ReadWrite<EquipItemRequest>(),
            ComponentType.ReadWrite<UnequipItemRequest>(),
            ComponentType.ReadWrite<ConsumeItemRequest>());

        if (query.CalculateEntityCount() != 1)
        {
            query.Dispose();
            return;
        }

        Entity player = query.GetSingletonEntity();

        DynamicBuffer<ItemCatalogEntry> catalog =
            em.GetBuffer<ItemCatalogEntry>(player, true);

        Entity sword = Entity.Null;

        for (int i = 0; i < catalog.Length; i++)
        {
            Entity item = catalog[i].ItemEntity;

            if (!em.Exists(item) ||
                !em.HasComponent<ItemDefinition>(item))
                continue;

            if (em.GetComponentData<ItemDefinition>(item).Id == 1)
            {
                sword = item;
                break;
            }
        }

        Entity potion = Entity.Null;

        for (int i = 0; i < catalog.Length; i++)
        {
            Entity item = catalog[i].ItemEntity;

            if (!em.Exists(item) ||
                !em.HasComponent<ItemDefinition>(item))
                continue;

            if (em.GetComponentData<ItemDefinition>(item).Id == 2)
            {
                potion = item;
                break;
            }
        }

        if (sword != Entity.Null)
        {
            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                em.GetBuffer<InventoryAddRequest>(player).Add(
                    new InventoryAddRequest
                    {
                        ItemEntity = sword,
                        Quantity = 1
                    });

                Debug.Log("Requested Iron Sword.");
            }

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                em.GetBuffer<EquipItemRequest>(player).Add(
                    new EquipItemRequest
                    {
                        ItemEntity = sword
                    });

                Debug.Log("Requested Iron Sword equip.");
            }

            if (Keyboard.current.uKey.wasPressedThisFrame)
            {
                em.GetBuffer<UnequipItemRequest>(player).Add(
                    new UnequipItemRequest
                    {
                        Slot = EquipmentSlot.Weapon
                    });

                Debug.Log("Requested Weapon unequip.");
            }
        }

        // P = Add one health potion.
        if (potion != Entity.Null &&
            Keyboard.current.pKey.wasPressedThisFrame)
        {
            em.GetBuffer<InventoryAddRequest>(player).Add(
                new InventoryAddRequest
                {
                    ItemEntity = potion,
                    Quantity = 1
                });

            Debug.Log("Requested Health Potion.");
        }

        // C = Consume one health potion.
        if (potion != Entity.Null &&
            Keyboard.current.cKey.wasPressedThisFrame)
        {
            em.GetBuffer<ConsumeItemRequest>(player).Add(
                new ConsumeItemRequest
                {
                    ItemEntity = potion
                });

            Debug.Log("Requested Health Potion consumption.");
        }

        query.Dispose();
    }
}