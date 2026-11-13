using Unity.Entities;
using UnityEngine;

public class WorldItemPickupAuthoring : MonoBehaviour
{
    public class Baker : Baker<WorldItemPickupAuthoring>
    {
        public override void Bake(WorldItemPickupAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new WorldItemPickup
            {
                ItemEntity = Entity.Null,
                Quantity = 0
            });
        }
    }
}