using UnityEngine;
using Fusion;

public class ArmorSystem : NetworkBehaviour
{
    private static readonly float[] DamageReduction = { 0.20f, 0.35f, 0.50f };
    private static readonly float[] MaxArmorHP      = { 100f,  150f,  200f  };

    [Networked] public int   Level   { get; private set; }   // 0 = no armor
    [Networked] public float ArmorHP { get; private set; }

    public bool HasArmor => Level > 0 && ArmorHP > 0f;

    public void EquipArmor(int level)
    {
        if (!HasStateAuthority || level < 1 || level > 3) return;
        if (level <= Level) return;  // never downgrade
        Level   = level;
        ArmorHP = MaxArmorHP[level - 1];
    }

    // Returns true HP damage after armor absorbs its share.
    // Armor HP = reduction * rawDamage per hit; breaks when depleted.
    public float AbsorbDamage(float rawDamage)
    {
        if (!HasArmor) return rawDamage;

        float reduction  = DamageReduction[Level - 1];
        float armorHit   = rawDamage * reduction;
        float hpDamage   = rawDamage * (1f - reduction);

        ArmorHP = Mathf.Max(0f, ArmorHP - armorHit);
        if (ArmorHP <= 0f) Level = 0;

        return hpDamage;
    }
}
