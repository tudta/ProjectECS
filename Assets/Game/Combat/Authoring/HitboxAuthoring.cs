using Unity.Entities;
using UnityEngine;

public class HitboxAuthoring : MonoBehaviour
{
    [Header("Hitbox")]
    public GameObject Owner;
    public float Damage = 25f;
    public DamageType Type = DamageType.Physical;

    public class Baker : Baker<HitboxAuthoring>
    {
        public override void Bake(HitboxAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            Entity owner = authoring.Owner != null
                ? GetEntity(authoring.Owner, TransformUsageFlags.Dynamic)
                : Entity.Null;

            AddComponent(entity, new HitboxData
            {
                Owner = owner,
                Damage = authoring.Damage,
                Type = authoring.Type
            });

            AddBuffer<HitEntity>(entity);
        }
    }
}