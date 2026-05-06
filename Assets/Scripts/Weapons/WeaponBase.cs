using UnityEngine;
using Fusion;

public class WeaponBase : NetworkBehaviour
{
    [SerializeField] protected WeaponData data;
    [SerializeField] protected Transform  muzzle;
    [SerializeField] protected NetworkPrefabRef bulletPrefab;

    [Networked] public int  CurrentAmmo  { get; protected set; }
    [Networked] public int  ReserveAmmo  { get; protected set; }
    [Networked] public bool IsReloading  { get; protected set; }
    [Networked] private TickTimer ReloadTimer   { get; set; }
    [Networked] private TickTimer FireRateTimer { get; set; }

    // Attachment levels (0 = none)
    [Networked] public int ScopeLevel { get; set; }
    [Networked] public int MagLevel   { get; set; }
    [Networked] public int GripLevel  { get; set; }

    public WeaponData Data => data;

    public override void Spawned()
    {
        CurrentAmmo = EffectiveMagSize;
        ReserveAmmo = EffectiveMagSize * 3;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        if (IsReloading && ReloadTimer.Expired(Runner)) FinishReload();
    }

    public virtual bool TryFire(bool isADS, float aimAssist)
    {
        if (!HasStateAuthority || IsReloading) return false;
        if (!FireRateTimer.ExpiredOrNotRunning(Runner)) return false;

        if (CurrentAmmo <= 0)
        {
            RPC_PlaySound(SoundType.Empty);
            TryReload();
            return false;
        }

        float interval = 60f / data.fireRate;
        FireRateTimer = TickTimer.CreateFromSeconds(Runner, interval);

        AntiCheat.Instance?.ValidateFireRate(Object.InputAuthority, interval);

        for (int i = 0; i < data.ammoPerShot; i++)
            SpawnBullet(muzzle.position, muzzle.forward + CalculateSpread(isADS));

        CurrentAmmo -= data.ammoPerShot;
        RPC_FireEffects();
        return true;
    }

    public void TryReload()
    {
        if (!HasStateAuthority || IsReloading) return;
        if (CurrentAmmo >= EffectiveMagSize || ReserveAmmo <= 0) return;

        IsReloading = true;
        float time  = GripLevel > 0 ? data.reloadTime * 0.85f : data.reloadTime;
        ReloadTimer = TickTimer.CreateFromSeconds(Runner, time);
        RPC_PlaySound(SoundType.Reload);
    }

    private void FinishReload()
    {
        int needed     = EffectiveMagSize - CurrentAmmo;
        int taken      = Mathf.Min(needed, ReserveAmmo);
        CurrentAmmo   += taken;
        ReserveAmmo   -= taken;
        IsReloading    = false;
    }

    private void SpawnBullet(Vector3 origin, Vector3 direction)
    {
        Runner.Spawn(
            bulletPrefab,
            origin,
            Quaternion.LookRotation(direction),
            inputAuthority: Object.InputAuthority,
            onBeforeSpawned: (_, obj) =>
            {
                obj.GetComponent<BulletController>()?.Initialize(
                    data.damage, data.headshotMultiplier,
                    data.effectiveRange, data.maxRange, data.rangeFalloff);
            });
    }

    private Vector3 CalculateSpread(bool isADS)
    {
        float spread = isADS ? data.adsSpread : data.hipfireSpread;
        if (GripLevel > 0) spread *= 0.8f;
        return new Vector3(
            UnityEngine.Random.Range(-spread, spread),
            UnityEngine.Random.Range(-spread, spread),
            0f) * Mathf.Deg2Rad;
    }

    private int EffectiveMagSize =>
        MagLevel > 0 ? Mathf.RoundToInt(data.magazineSize * 1.33f) : data.magazineSize;

    private enum SoundType { Fire, Reload, Empty }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_FireEffects()
        => AudioManager.Instance?.PlayAt(data.fireSound, muzzle.position);

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlaySound(SoundType type)
    {
        AudioClip clip = type switch
        {
            SoundType.Reload => data.reloadSound,
            SoundType.Empty  => data.emptySound,
            _                => null,
        };
        if (clip != null) AudioManager.Instance?.PlayAt(clip, transform.position);
    }
}
