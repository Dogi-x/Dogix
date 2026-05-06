using UnityEngine;
using Fusion;

public class BulletController : NetworkBehaviour
{
    [SerializeField] private float speed = 400f;
    [SerializeField] private LayerMask hitMask;

    private float          _damage;
    private float          _headshotMultiplier;
    private float          _effectiveRange;
    private float          _maxRange;
    private AnimationCurve _falloff;
    private float          _distanceTraveled;

    public void Initialize(float damage, float headshotMult, float effectiveRange, float maxRange, AnimationCurve falloff)
    {
        _damage             = damage;
        _headshotMultiplier = headshotMult;
        _effectiveRange     = effectiveRange;
        _maxRange           = maxRange;
        _falloff            = falloff;
    }

    public override void FixedUpdateNetwork()
    {
        float stepDist = speed * Runner.DeltaTime;

        if (Runner.LagCompensation.Raycast(
            transform.position, transform.forward, stepDist,
            Object.InputAuthority, out LagCompensatedHit hit, hitMask))
        {
            ProcessHit(hit);
            Runner.Despawn(Object);
            return;
        }

        transform.position   += transform.forward * stepDist;
        _distanceTraveled    += stepDist;

        if (_distanceTraveled >= _maxRange) Runner.Despawn(Object);
    }

    private void ProcessHit(LagCompensatedHit hit)
    {
        float t = Mathf.Clamp01(_distanceTraveled / Mathf.Max(0.001f, _effectiveRange));
        float dmg = _damage * _falloff.Evaluate(t);

        bool isHeadshot = hit.GameObject != null && hit.GameObject.CompareTag("Head");
        if (isHeadshot) dmg *= _headshotMultiplier;

        AntiCheat.Instance?.ValidateDamage(Object.InputAuthority, dmg);

        hit.GameObject?.GetComponentInParent<PlayerHealth>()?.
            TakeDamage(dmg, DamageSource.Bullet);

        hit.GameObject?.GetComponent<GateController>()?.TakeDamage(dmg);
    }
}
