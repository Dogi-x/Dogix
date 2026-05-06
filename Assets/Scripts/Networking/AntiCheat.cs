using UnityEngine;
using Fusion;
using System.Collections.Generic;

// Server-only. Validates speed, fire rate, and damage on every relevant action.
// Kicks after KICK_THRESHOLD violations. Client values are never trusted.
public class AntiCheat : NetworkBehaviour
{
    public static AntiCheat Instance { get; private set; }

    private const float SPEED_MARGIN       = 1.30f;  // 30% tolerance for lag
    private const float FIRE_RATE_MARGIN   = 0.85f;  // must wait 85% of weapon interval
    private const float MAX_HIT_DAMAGE     = 200f;   // headshot sniper ceiling
    private const int   KICK_THRESHOLD     = 5;

    private Dictionary<PlayerRef, int>     _violations   = new();
    private Dictionary<PlayerRef, float>   _lastFireTime = new();
    private Dictionary<PlayerRef, Vector3> _lastPos      = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ValidateSpeed(PlayerRef player, Vector3 newPos, float maxSpeed)
    {
        if (!HasStateAuthority) return;
        if (!_lastPos.TryGetValue(player, out var last)) { _lastPos[player] = newPos; return; }

        float moved   = Vector3.Distance(last, newPos);
        float allowed = maxSpeed * SPEED_MARGIN * Runner.DeltaTime;

        if (moved > allowed + 0.5f)   // +0.5m absolute tolerance
            Flag(player, $"Speed: {moved:F2}m > {allowed:F2}m");

        _lastPos[player] = newPos;
    }

    public void ValidateFireRate(PlayerRef player, float weaponInterval)
    {
        if (!HasStateAuthority) return;
        float now = Runner.SimulationTime;

        if (_lastFireTime.TryGetValue(player, out float last))
        {
            float elapsed = now - last;
            if (elapsed < weaponInterval * FIRE_RATE_MARGIN)
                Flag(player, $"FireRate: {elapsed:F3}s < min {weaponInterval * FIRE_RATE_MARGIN:F3}s");
        }
        _lastFireTime[player] = now;
    }

    public void ValidateDamage(PlayerRef player, float damage)
    {
        if (!HasStateAuthority) return;
        if (damage > MAX_HIT_DAMAGE)
            Flag(player, $"Damage: {damage:F1} > max {MAX_HIT_DAMAGE}");
    }

    private void Flag(PlayerRef player, string reason)
    {
        _violations.TryAdd(player, 0);
        _violations[player]++;
        Debug.LogWarning($"[AntiCheat] {player} violation {_violations[player]}: {reason}");

        if (_violations[player] >= KICK_THRESHOLD)
        {
            Debug.Log($"[AntiCheat] Kicking {player}");
            Runner.Disconnect(player);
        }
    }
}
