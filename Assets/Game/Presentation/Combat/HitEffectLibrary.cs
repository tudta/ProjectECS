using UnityEngine;

public class HitEffectLibrary : MonoBehaviour
{
    [Header("Physical")]
    public GameObject physicalHitPrefab;
    public AudioClip physicalHitSound;

    [Header("Elemental")]
    public GameObject fireHitPrefab;
    public AudioClip fireHitSound;

    public GameObject coldHitPrefab;
    public AudioClip coldHitSound;

    public GameObject lightningHitPrefab;
    public AudioClip lightningHitSound;

    public GameObject poisonHitPrefab;
    public AudioClip poisonHitSound;

    [Header("Death")]
    public GameObject deathPrefab;
    public AudioClip deathSound;

    public GameObject GetHitPrefab(DamageType type)
    {
        return type switch
        {
            DamageType.Physical => physicalHitPrefab,
            DamageType.Fire => fireHitPrefab,
            DamageType.Cold => coldHitPrefab,
            DamageType.Lightning => lightningHitPrefab,
            DamageType.Poison => poisonHitPrefab,
            _ => null
        };
    }

    public AudioClip GetHitSound(DamageType type)
    {
        return type switch
        {
            DamageType.Physical => physicalHitSound,
            DamageType.Fire => fireHitSound,
            DamageType.Cold => coldHitSound,
            DamageType.Lightning => lightningHitSound,
            DamageType.Poison => poisonHitSound,
            _ => null
        };
    }
}