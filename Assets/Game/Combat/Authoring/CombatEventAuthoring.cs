using Unity.Entities;
using UnityEngine;

public class CombatEventAuthoring : MonoBehaviour
{
    public class Baker : Baker<CombatEventAuthoring>
    {
        public override void Bake(CombatEventAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddBuffer<DamageRequest>(entity);
            AddBuffer<DamageReceivedEvent>(entity);
            AddBuffer<DeathEvent>(entity);
        }
    }
}