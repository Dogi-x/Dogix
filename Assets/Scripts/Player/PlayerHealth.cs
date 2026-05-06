using UnityEngine;
using Fusion;
using System;

public enum DamageSource { Bullet, Explosion, Zone }

public class PlayerHealth : NetworkBehaviour
{
    public const float MAX_HP = 100f;
    private const float REVIVE_WINDOW = 30f;

    [Networked] public float HP          { get; private set; }
    [Networked] public bool  IsKnocked   { get; private set; }
    [Networked] public bool  IsDead      { get; private set; }
    [Networked] private TickTimer KnockTimer { get; set; }

    private ArmorSystem    _armor;
    private HealingSystem  _healing;
    private TeamType       _team;

    public event Action<float, float> OnHealthChanged;
    public event Action OnKnocked;
    public event Action OnRevived;
    public event Action OnDied;

    public override void Spawned()
    {
        HP      = MAX_HP;
        _armor  = GetComponent<ArmorSystem>();
        _healing = GetComponent<HealingSystem>();
    }

    public void Initialize(TeamType team) => _team = team;

    public void TakeDamage(float rawDamage, DamageSource source)
    {
        if (!HasStateAuthority || IsDead) return;

        // Interrupt any active healing
        _healing?.InterruptByDamage();

        float actual = (_armor != null && source != DamageSource.Zone)
            ? _armor.AbsorbDamage(rawDamage)
            : rawDamage;

        HP = Mathf.Max(0f, HP - actual);
        RPC_HitFeedback(source);
        OnHealthChanged?.Invoke(HP, MAX_HP);

        if (HP <= 0f && !IsKnocked)
            Knock();
    }

    public void Heal(float amount)
    {
        if (!HasStateAuthority || IsDead || IsKnocked) return;
        HP = Mathf.Min(MAX_HP, HP + amount);
        OnHealthChanged?.Invoke(HP, MAX_HP);
    }

    private void Knock()
    {
        IsKnocked  = true;
        KnockTimer = TickTimer.CreateFromSeconds(Runner, REVIVE_WINDOW);
        RPC_OnKnocked();
    }

    public void Revive()
    {
        if (!HasStateAuthority || !IsKnocked || IsDead) return;
        IsKnocked = false;
        HP        = 30f;
        RPC_OnRevived();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || !IsKnocked || IsDead) return;
        if (KnockTimer.Expired(Runner)) Die();
    }

    private void Die()
    {
        IsDead    = true;
        IsKnocked = false;
        MatchManager.Instance?.PlayerDied(Object.InputAuthority, _team);
        RPC_OnDied();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_HitFeedback(DamageSource source)
    {
        HUDController.Instance?.ShowHitMarker();
        AudioManager.Instance?.PlayHitSound(source);
        if (HasInputAuthority) CameraShake.Shake(0.12f);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_OnKnocked() => OnKnocked?.Invoke();

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_OnRevived() => OnRevived?.Invoke();

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_OnDied() => OnDied?.Invoke();
}
