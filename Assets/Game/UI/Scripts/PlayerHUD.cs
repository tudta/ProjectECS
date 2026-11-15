using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TMP_Text healthText;

    [Header("Experience")]
    [SerializeField] private Slider experienceBar;
    [SerializeField] private TMP_Text experienceText;

    [Header("Level")]
    [SerializeField] private TMP_Text levelText;

    [Header("Skills")]
    [SerializeField] private Image skillCooldownOverlay;
    [SerializeField] private TMP_Text skillCooldownText;

    private World world;
    private EntityQuery playerQuery;
    private bool queryCreated;

    private void Update()
    {
        World currentWorld =
            World.DefaultGameObjectInjectionWorld;

        if (currentWorld == null || !currentWorld.IsCreated)
            return;

        // Rebuild the query if the ECS world changes.
        if (world != currentWorld)
        {
            DisposeQuery();

            world = currentWorld;

            playerQuery = world.EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly<PlayerTag>(),
                ComponentType.ReadOnly<Health>(),
                ComponentType.ReadOnly<CharacterExperience>(),
                ComponentType.ReadOnly<CharacterLevel>(),
                ComponentType.ReadOnly<SkillCaster>()
            );

            queryCreated = true;
        }

        if (playerQuery.CalculateEntityCount() != 1)
            return;

        EntityManager em = world.EntityManager;
        Entity player = playerQuery.GetSingletonEntity();

        Health health = em.GetComponentData<Health>(player);

        CharacterExperience experience =
            em.GetComponentData<CharacterExperience>(player);

        CharacterLevel level =
            em.GetComponentData<CharacterLevel>(player);

        SkillCaster skillCaster =
            em.GetComponentData<SkillCaster>(player);

        UpdateHealth(health);
        UpdateExperience(experience, level);
        UpdateLevel(level);
        UpdateSkillCooldown(skillCaster);
    }

    private void UpdateHealth(Health health)
    {
        if (healthBar != null)
        {
            healthBar.minValue = 0f;
            healthBar.maxValue = Mathf.Max(1f, health.Maximum);
            healthBar.value = health.Current;
        }

        if (healthText != null)
        {
            healthText.text =
                $"{Mathf.CeilToInt(health.Current)} / " +
                $"{Mathf.CeilToInt(health.Maximum)}";
        }
    }

    private void UpdateExperience(
        CharacterExperience experience,
        CharacterLevel level)
    {
        int requiredXP = Mathf.Max(
            1,
            level.BaseExperience * Mathf.Max(1, level.Value)
        );

        if (experienceBar != null)
        {
            experienceBar.minValue = 0f;
            experienceBar.maxValue = requiredXP;
            experienceBar.value = experience.Current;
        }

        if (experienceText != null)
        {
            experienceText.text =
                $"{experience.Current} / {requiredXP}";
        }
    }

    private void UpdateLevel(CharacterLevel level)
    {
        if (levelText != null)
            levelText.text = $"Level {level.Value}";
    }

    private void UpdateSkillCooldown(SkillCaster skill)
    {
        float cooldown = Mathf.Max(0f, skill.Cooldown);
        float remaining = Mathf.Clamp(
            skill.CooldownRemaining,
            0f,
            cooldown
        );

        bool onCooldown = cooldown > 0f && remaining > 0.01f;

        if (skillCooldownOverlay != null)
        {
            skillCooldownOverlay.gameObject.SetActive(onCooldown);

            skillCooldownOverlay.fillAmount =
                onCooldown ? remaining / cooldown : 0f;
        }

        if (skillCooldownText != null)
        {
            skillCooldownText.text = onCooldown
                ? Mathf.CeilToInt(remaining).ToString()
                : "Q";

            if (remaining == 0)
            {
                skillCooldownText.text = string.Empty;
            }
        }
    }

    private void OnDestroy()
    {
        DisposeQuery();
    }

    private void DisposeQuery()
    {
        if (!queryCreated)
            return;

        if (world != null && world.IsCreated)
            playerQuery.Dispose();

        queryCreated = false;
    }
}