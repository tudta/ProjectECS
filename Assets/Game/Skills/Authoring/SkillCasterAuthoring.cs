using Unity.Entities;
using UnityEngine;

public class SkillCasterAuthoring : MonoBehaviour
{
    public GameObject SkillPrefab;

    public float Cooldown = 0.5f;

    public class Baker : Baker<SkillCasterAuthoring>
    {
        public override void Bake(SkillCasterAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            Entity prefab = GetEntity(
                authoring.SkillPrefab,
                TransformUsageFlags.Dynamic
            );

            AddComponent(entity, new SkillCaster
            {
                SkillPrefab = prefab,
                Cooldown = Mathf.Max(0f, authoring.Cooldown),
                CooldownRemaining = 0f
            });

            AddBuffer<SkillCastRequest>(entity);
        }
    }
}