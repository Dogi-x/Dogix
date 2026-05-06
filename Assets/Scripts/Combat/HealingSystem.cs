using UnityEngine;
using Fusion;

public enum HealItemType { Bandage, Medkit }

public class HealingSystem : NetworkBehaviour
{
    private const float BANDAGE_HEAL = 10f;
    private const float BANDAGE_TIME = 4f;
    private const float MEDKIT_HEAL  = 100f;
    private const float MEDKIT_TIME  = 6f;

    [Networked] public bool IsHealing { get; private set; }
    [Networked] private TickTimer     HealTimer       { get; set; }
    [Networked] private HealItemType  CurrentHealItem { get; set; }

    private PlayerHealth    _health;
    private PlayerController _controller;

    public override void Spawned()
    {
        _health     = GetComponent<PlayerHealth>();
        _controller = GetComponent<PlayerController>();
    }

    public void TryHeal(HealItemType itemType)
    {
        if (!HasStateAuthority || IsHealing) return;
        if (_health == null || _health.IsKnocked || _health.IsDead) return;
        if (_health.HP >= PlayerHealth.MAX_HP) return;

        CurrentHealItem = itemType;
        float duration  = itemType == HealItemType.Medkit ? MEDKIT_TIME : BANDAGE_TIME;
        IsHealing  = true;
        HealTimer  = TickTimer.CreateFromSeconds(Runner, duration);
    }

    public void InterruptByDamage()
    {
        if (HasStateAuthority && IsHealing) IsHealing = false;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || !IsHealing) return;

        // Cancel on sprint
        if (_controller != null && _controller.IsSprinting)
        {
            IsHealing = false;
            return;
        }

        if (HealTimer.Expired(Runner))
        {
            IsHealing = false;
            float amount = CurrentHealItem == HealItemType.Medkit ? MEDKIT_HEAL : BANDAGE_HEAL;
            _health?.Heal(amount);
        }
    }
}
