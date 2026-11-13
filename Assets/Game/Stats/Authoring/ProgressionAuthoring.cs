using Unity.Entities;
using UnityEngine;

public class ProgressionAuthoring : MonoBehaviour
{
    [Header("Starting Progression")]
    public int StartingLevel = 1;
    public int BaseExperience = 100;

    [Header("Stats Gained Per Level")]
    public float MaxHealthPerLevel = 10f;
    public float MovementSpeedPerLevel = 0.1f;
    public float ArmorPerLevel = 1f;
    public float AttackDamagePerLevel = 2f;

    public class Baker : Baker<ProgressionAuthoring>
    {
        public override void Bake(ProgressionAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new CharacterExperience
            {
                Current = 0,
                Lifetime = 0
            });

            AddComponent(entity, new CharacterLevel
            {
                Value = Mathf.Max(1, authoring.StartingLevel),
                BaseExperience = Mathf.Max(1, authoring.BaseExperience),

                MaxHealthPerLevel = authoring.MaxHealthPerLevel,
                MovementSpeedPerLevel = authoring.MovementSpeedPerLevel,
                ArmorPerLevel = authoring.ArmorPerLevel,
                AttackDamagePerLevel = authoring.AttackDamagePerLevel
            });

            AddBuffer<ExperienceGainRequest>(entity);
        }
    }
}