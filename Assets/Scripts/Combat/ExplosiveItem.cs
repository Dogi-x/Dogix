using UnityEngine;
using Fusion;

public enum ExplosiveType { Grenade, Smoke, Flash }

public class ExplosiveItem : NetworkBehaviour
{
    [SerializeField] private ExplosiveType type          = ExplosiveType.Grenade;
    [SerializeField] private float         fuseTime      = 3f;
    [SerializeField] private float         blastRadius   = 5f;
    [SerializeField] private float         grenadeDamage = 80f;
    [SerializeField] private float         smokeDuration = 8f;
    [SerializeField] private float         flashDuration = 2.5f;
    [SerializeField] private LayerMask     playerMask;
    [SerializeField] private GameObject    smokeVfxPrefab;
    [SerializeField] private GameObject    explosionVfxPrefab;

    [Networked] private TickTimer FuseTimer   { get; set; }
    [Networked] public bool       HasExploded { get; private set; }

    public void Throw(Vector3 velocity)
    {
        if (!HasStateAuthority) return;
        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.velocity = velocity;
        FuseTimer = TickTimer.CreateFromSeconds(Runner, fuseTime);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || HasExploded) return;
        if (FuseTimer.Expired(Runner)) Explode();
    }

    private void Explode()
    {
        HasExploded = true;
        switch (type)
        {
            case ExplosiveType.Grenade: ExplodeGrenade(); break;
            case ExplosiveType.Smoke:   ExplodeSmoke();   break;
            case ExplosiveType.Flash:   ExplodeFlash();   break;
        }
        Runner.Despawn(Object);
    }

    private void ExplodeGrenade()
    {
        RPC_GrenadeEffects(transform.position);
        Collider[] hits = Physics.OverlapSphere(transform.position, blastRadius, playerMask);
        foreach (var col in hits)
        {
            var health = col.GetComponentInParent<PlayerHealth>();
            if (health == null || health.IsDead) continue;
            float dist = Vector3.Distance(transform.position, col.transform.position);
            float dmg  = DamageSystem.ExplosionFalloff(grenadeDamage, dist, blastRadius);
            if (dmg > 0f) health.TakeDamage(dmg, DamageSource.Explosion);
        }
    }

    private void ExplodeSmoke() => RPC_SmokeEffects(transform.position, smokeDuration);
    private void ExplodeFlash() => RPC_FlashEffects(transform.position, flashDuration);

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_GrenadeEffects(Vector3 pos)
    {
        if (explosionVfxPrefab != null) Instantiate(explosionVfxPrefab, pos, Quaternion.identity);
        AudioManager.Instance?.PlayExplosionAt(pos);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SmokeEffects(Vector3 pos, float duration)
    {
        if (smokeVfxPrefab != null) Destroy(Instantiate(smokeVfxPrefab, pos, Quaternion.identity), duration);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_FlashEffects(Vector3 pos, float duration)
    {
        if (!HasInputAuthority) return;
        float dist = Vector3.Distance(pos, Camera.main.transform.position);
        if (dist < blastRadius)
        {
            float intensity = 1f - (dist / blastRadius);
            HUDController.Instance?.TriggerFlash(intensity, duration);
        }
    }
}
