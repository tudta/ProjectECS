using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(DeathSystem))]
[UpdateBefore(typeof(ExperienceSystem))]
public partial struct ExperienceRewardSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityManager em = state.EntityManager;

        // Find the player who receives XP.
        if (!SystemAPI.TryGetSingletonEntity<PlayerTag>(
                out Entity player))
            return;

        if (!em.HasBuffer<ExperienceGainRequest>(player))
            return;

        DynamicBuffer<ExperienceGainRequest> requests =
            em.GetBuffer<ExperienceGainRequest>(player);

        EntityCommandBuffer ecb =
            new EntityCommandBuffer(Allocator.Temp);

        foreach (var (reward, enemyEntity) in
            SystemAPI.Query<RefRO<ExperienceReward>>()
                .WithAll<CharacterTag, DeadTag>()
                .WithNone<ExperienceRewardProcessedTag>()
                .WithEntityAccess())
        {
            int amount = reward.ValueRO.Amount;

            if (amount > 0)
            {
                requests.Add(new ExperienceGainRequest
                {
                    Amount = amount,
                    Source = enemyEntity
                });
            }

            // Prevent this enemy from awarding XP again.
            ecb.AddComponent<ExperienceRewardProcessedTag>(
                enemyEntity);
        }

        ecb.Playback(em);
        ecb.Dispose();
    }
}