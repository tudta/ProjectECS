using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct GameBootstrapSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameInitialized>();
    }

    public void OnUpdate(ref SystemState state)
    {
        Debug.Log("ARPG ECS World Initialized!");

        // Only execute once.
        state.Enabled = false;
    }
}