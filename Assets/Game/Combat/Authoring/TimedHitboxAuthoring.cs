using Unity.Entities;
using UnityEngine;

public class TimedHitboxAuthoring : MonoBehaviour
{
    public float Lifetime = 0.2f;

    public class Baker : Baker<TimedHitboxAuthoring>
    {
        public override void Bake(TimedHitboxAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            float lifetime = Mathf.Max(0.05f, authoring.Lifetime);

            AddComponent(entity, new SkillLifetime
            {
                Remaining = lifetime
            });
        }
    }
}