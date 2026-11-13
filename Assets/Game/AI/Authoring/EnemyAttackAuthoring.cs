using Unity.Entities;
using UnityEngine;

public class EnemyAttackAuthoring : MonoBehaviour
{
    [Header("Attack")]
    public GameObject HitboxPrefab;

    public float Cooldown = 1.2f;

    public float ForwardOffset = 0.8f;

    public class Baker : Baker<EnemyAttackAuthoring>
    {
        public override void Bake(EnemyAttackAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            Entity prefab = GetEntity(
                authoring.HitboxPrefab,
                TransformUsageFlags.Dynamic
            );

            AddComponent(entity, new EnemyAttackSettings
            {
                HitboxPrefab = prefab,
                Cooldown = Mathf.Max(0.05f, authoring.Cooldown),
                CooldownRemaining = 0f,
                ForwardOffset = authoring.ForwardOffset
            });
        }
    }
}