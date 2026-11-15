using Unity.Entities;
using UnityEngine;

public class ExperienceRewardAuthoring : MonoBehaviour
{
    [Min(0)]
    public int Experience = 25;

    public class Baker : Baker<ExperienceRewardAuthoring>
    {
        public override void Bake(ExperienceRewardAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new ExperienceReward
            {
                Amount = Mathf.Max(0, authoring.Experience)
            });
        }
    }
}