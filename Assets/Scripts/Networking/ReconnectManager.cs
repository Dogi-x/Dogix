using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class ReconnectManager : NetworkBehaviour
{
    public static ReconnectManager Instance { get; private set; }

    private const float RECONNECT_WINDOW = 60f;

    private struct Snapshot
    {
        public Vector3 Position;
        public float   HP;
        public float   DisconnectTime;
    }

    private Dictionary<PlayerRef, Snapshot> _snapshots = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void HandleDisconnect(PlayerRef player)
    {
        if (!HasStateAuthority) return;
        var obj = Runner.GetPlayerObject(player);
        if (obj == null) return;

        var health = obj.GetComponent<PlayerHealth>();
        if (health == null || health.IsDead) return;

        _snapshots[player] = new Snapshot
        {
            Position      = obj.transform.position,
            HP            = health.HP,
            DisconnectTime = Runner.SimulationTime,
        };
    }

    public void HandleReconnect(PlayerRef player)
    {
        if (!HasStateAuthority) return;
        if (!_snapshots.TryGetValue(player, out var snap)) return;

        float elapsed = Runner.SimulationTime - snap.DisconnectTime;
        _snapshots.Remove(player);

        if (elapsed > RECONNECT_WINDOW) return;  // window expired — already handled by FixedUpdate

        var obj = Runner.GetPlayerObject(player);
        if (obj == null) return;

        var cc = obj.GetComponent<CharacterController>();
        if (cc != null) { cc.enabled = false; obj.transform.position = snap.Position; cc.enabled = true; }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        List<PlayerRef> expired = null;
        foreach (var kv in _snapshots)
        {
            if (Runner.SimulationTime - kv.Value.DisconnectTime > RECONNECT_WINDOW)
                (expired ??= new List<PlayerRef>()).Add(kv.Key);
        }

        if (expired == null) return;
        foreach (var p in expired)
        {
            // Window closed — kill the lingering player object
            Runner.GetPlayerObject(p)?.
                GetComponent<PlayerHealth>()?.TakeDamage(9999f, DamageSource.Zone);
            _snapshots.Remove(p);
        }
    }
}
