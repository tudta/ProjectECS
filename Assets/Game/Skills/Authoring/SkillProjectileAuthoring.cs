using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SkillProjectileAuthoring : MonoBehaviour
{
    [Header("Projectile")]
    public Vector3 SpawnOffset = new Vector3(0f, 0f, 1f);

    public float Speed = 12f;

    public float Lifetime = 3f;

    public class Baker : Baker<SkillProjectileAuthoring>
    {
        public override void Bake(SkillProjectileAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            float lifetime = Mathf.Max(0.05f, authoring.Lifetime);

            AddComponent(entity, new SkillProjectileData
            {
                SpawnOffset = (float3)authoring.SpawnOffset,
                Speed = Mathf.Max(0f, authoring.Speed),
                Lifetime = lifetime
            });

            AddComponent(entity, new SkillLifetime
            {
                Remaining = lifetime
            });
        }
    }
}