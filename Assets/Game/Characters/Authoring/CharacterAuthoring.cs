
using Unity.Entities;
using UnityEngine;

public class CharacterAuthoring : MonoBehaviour
{
    [Header("Character")]
    public bool IsPlayer;

    [Tooltip("0 = Player, 1 = Enemy")]
    public int Faction;

    [Header("Stats")]
    public float MaxHealth = 100f;
    public float MovementSpeed = 5f;
    public float Armor;

    public class Baker : Baker<CharacterAuthoring>
    {
        public override void Bake(CharacterAuthoring authoring)
        {
            Entity entity = GetEntity(
                TransformUsageFlags.Dynamic
            );

            AddComponent<CharacterTag>(entity);
            AddComponent<CharacterMoveCommand>(entity);

            if (authoring.IsPlayer)
            {
                AddComponent<PlayerTag>(entity);
            }

            AddComponent(entity, new CharacterFaction
            {
                Value = authoring.Faction
            });

            AddComponent(entity, new Health
            {
                Current = authoring.MaxHealth,
                Maximum = authoring.MaxHealth
            });

            AddComponent(entity, new MovementSpeed
            {
                Value = authoring.MovementSpeed
            });

            AddComponent(entity, new Armor
            {
                Value = authoring.Armor
            });
        }
    }
}