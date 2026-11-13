using Unity.Entities;
using UnityEngine;

public class CharacterStatsAuthoring : MonoBehaviour
{
    [Header("Base Stats")]
    public float MaxHealth = 100f;
    public float MovementSpeed = 5f;
    public float Armor = 0f;
    public float AttackDamage = 10f;

    public class Baker : Baker<CharacterStatsAuthoring>
    {
        public override void Bake(CharacterStatsAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new CharacterBaseStats
            {
                MaxHealth = authoring.MaxHealth,
                MovementSpeed = authoring.MovementSpeed,
                Armor = authoring.Armor,
                AttackDamage = authoring.AttackDamage
            });

            AddComponent(entity, new CharacterDerivedStats
            {
                MaxHealth = authoring.MaxHealth,
                MovementSpeed = authoring.MovementSpeed,
                Armor = authoring.Armor,
                AttackDamage = authoring.AttackDamage
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

            AddBuffer<CharacterStatModifier>(entity);
        }
    }
}