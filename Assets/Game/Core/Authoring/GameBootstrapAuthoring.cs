using Unity.Entities;
using UnityEngine;

public class GameBootstrapAuthoring : MonoBehaviour
{
    public class Baker : Baker<GameBootstrapAuthoring>
    {
        public override void Bake(GameBootstrapAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent<GameInitialized>(entity);
        }
    }
}