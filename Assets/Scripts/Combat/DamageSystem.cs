using UnityEngine;

// Stateless helper used wherever damage must be calculated outside weapon code.
public static class DamageSystem
{
    public static float Calculate(float rawDamage, ArmorSystem armor, DamageSource source)
    {
        if (armor == null || source == DamageSource.Zone)
            return rawDamage;
        return armor.AbsorbDamage(rawDamage);
    }

    // Quadratic falloff from explosion center to radius edge.
    public static float ExplosionFalloff(float damage, float distance, float radius)
    {
        if (distance >= radius) return 0f;
        float t = 1f - (distance / radius);
        return damage * t * t;
    }
}
