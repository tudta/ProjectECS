using Unity.Entities;
using UnityEngine;

public class EnemyAIAuthoring : MonoBehaviour
{
    [Header("Detection")]
    public float AggroRange = 10f;

    [Header("Combat")]
    public float AttackRange = 1.5f;

    public class Baker : Baker<EnemyAIAuthoring>
    {
        public override void Bake(EnemyAIAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<EnemyAITag>(entity);

            AddComponent(entity, new EnemyAISettings
            {
                AggroRange = Mathf.Max(0f, authoring.AggroRange),
                AttackRange = Mathf.Max(0f, authoring.AttackRange)
            });

            AddComponent(entity, new EnemyAITarget
            {
                Value = Entity.Null
            });

            AddComponent(entity, new EnemyAIState
            {
                IsAggro = 0
            });
        }
    }
}